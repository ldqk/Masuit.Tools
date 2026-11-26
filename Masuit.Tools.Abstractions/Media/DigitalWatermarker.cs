using System.Buffers;
using System.Text;
using SkiaSharp;

namespace Masuit.Tools.Media;

/// <summary>使用 Haar 小波、DCT 和 SVD 将盲水印嵌入图片，或从图片中提取水印。</summary>
/// <remarks>
/// 提取时须使用与嵌入时相同的密码和强度参数，并提供原始水印长度或尺寸。
/// 水印容量为 <c>(ceil(图片高度 / 2) / 4) * (ceil(图片宽度 / 2) / 4)</c> 位，
/// 其中除以 4 的结果向下取整。密码用于确定性置乱，不提供密码学安全保证。
/// 水印不包含校验码；缩放、压缩或参数错误可能产生无法识别的错误结果。
/// </remarks>
public sealed class DigitalWatermarker
{
    private const int BlockSize = 4;
    private const double Epsilon = 1e-10;
    private static readonly double[] DctBasis = CreateDctBasis();
    private readonly double _d1;
    private readonly double _d2;

    /// <summary>创建水印处理器。</summary>
    /// <param name="imagePassword">控制每个图片块中 DCT 系数的置乱顺序。</param>
    /// <param name="watermarkPassword">控制水印位的置乱顺序。</param>
    /// <param name="d1">第一奇异值的量化步长，必须大于零。</param>
    /// <param name="d2">第二奇异值的量化步长；为零时不使用第二奇异值。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="d1"/> 不大于零，或 <paramref name="d2"/> 小于零。</exception>
    public DigitalWatermarker(int imagePassword = 1, int watermarkPassword = 1, double d1 = 36, double d2 = 20)
    {
        if (d1 <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(d1), "d1 必须大于零。");
        }

        if (d2 < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(d2), "d2 不能小于零。");
        }

        ImagePassword = imagePassword;
        WatermarkPassword = watermarkPassword;
        _d1 = d1;
        _d2 = d2;
    }

    /// <summary>获取控制图片块系数置乱顺序的密码。</summary>
    public int ImagePassword { get; }

    /// <summary>获取控制水印位置乱顺序的密码。</summary>
    public int WatermarkPassword { get; }

    /// <summary>将非空文本按 UTF-8 编码后嵌入载体图片。</summary>
    /// <param name="image">载体图片，不会被修改。</param>
    /// <param name="text">要嵌入的非空文本。</param>
    /// <returns>包含水印的新图片；调用者负责释放。</returns>
    /// <exception cref="ArgumentNullException">载体图片或文本为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">文本为空、图片尺寸无效或图片容量不足。</exception>
    public SKBitmap EmbedText(SKBitmap image, string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text), "水印文本不能为空。");
        }
        if (text.Length == 0)
        {
            throw new ArgumentException("水印文本不能为空。", nameof(text));
        }

        return EmbedBitSequence(image, EncodeBytes(Encoding.UTF8.GetBytes(text)));
    }

    /// <summary>将字节形式的二进制水印嵌入载体图片。</summary>
    /// <param name="image">载体图片，不会被修改。</param>
    /// <param name="watermark">非空水印字节数组，不会被修改。</param>
    /// <returns>包含水印的新图片；调用者负责释放。</returns>
    /// <remarks>每个字节按最高位优先展开为 8 个水印位；提取时需提供相同的字节数。</remarks>
    /// <exception cref="ArgumentNullException">载体图片或水印为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">水印为空、图片尺寸无效或图片容量不足。</exception>
    /// <exception cref="OverflowException">水印字节数转换为位数时溢出。</exception>
    public SKBitmap EmbedBits(SKBitmap image, byte[] watermark)
    {
        if (watermark is null)
        {
            throw new ArgumentNullException(nameof(watermark), "水印字节数组不能为空。");
        }

        if (watermark.Length == 0)
        {
            throw new ArgumentException("水印必须至少包含一个字节。", nameof(watermark));
        }

        return EmbedBitSequence(image, EncodeBytes(watermark));
    }

    private SKBitmap EmbedBitSequence(SKBitmap image, bool[] watermark)
    {
        if (watermark.Length == 0)
        {
            throw new ArgumentException("水印必须至少包含一位。", nameof(watermark));
        }

        Shuffle(watermark, CreateRandom(WatermarkPassword));
        return Process(image, watermark);
    }

    /// <summary>将图片水印二值化后嵌入载体图片，每个水印像素占一位。</summary>
    /// <param name="image">载体图片，不会被修改。</param>
    /// <param name="watermark">水印图片，不会被修改。</param>
    /// <returns>包含水印的新图片；调用者负责释放。</returns>
    /// <remarks>RGB 三个分量的平均值大于 128 时记为白色，否则记为黑色；不保存颜色或透明度。水印像素越多，通常可用于重复嵌入的块越少，抗损坏能力可能下降。</remarks>
    /// <exception cref="ArgumentNullException">载体图片或水印图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">水印为空、载体图片尺寸无效或图片容量不足。</exception>
    /// <exception cref="OverflowException">水印图片的像素数超出可表示的范围。</exception>
    public SKBitmap EmbedImage(SKBitmap image, SKBitmap watermark)
    {
        if (watermark is null)
        {
            throw new ArgumentNullException(nameof(watermark), "水印图片不能为空。");
        }
        var bits = new bool[checked(watermark.Width * watermark.Height)];
        var index = 0;
        for (var y = 0; y < watermark.Height; y++)
        {
            for (var x = 0; x < watermark.Width; x++)
            {
                var color = watermark.GetPixel(x, y);
                bits[index++] = (color.Red + color.Green + color.Blue) / 3 > 128;
            }
        }

        return EmbedBitSequence(image, bits);
    }

    /// <summary>按当前图片尺寸和方向提取水印位的软判决值。</summary>
    /// <param name="image">包含水印的图片；本方法不自动恢复旋转或缩放。</param>
    /// <param name="bitCount">嵌入时的水印位数，必须大于零。</param>
    /// <returns>已恢复原水印顺序的值，范围为 0 到 1；越接近 1 越倾向于表示位 1，并非概率。</returns>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitCount"/> 不大于零。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或不足以容纳指定的水印位数。</exception>
    public double[] ExtractSoftBits(SKBitmap image, int bitCount)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }
        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new ArgumentException("载体图片的宽度和高度必须大于零。", nameof(image));
        }

        if (bitCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), "水印位数必须大于零。");
        }

        ValidateCapacity(bitCount, GetBlockCount(image.Width, image.Height), image.Width, image.Height);
        using var buffer = new LowBuffer(image.Width, image.Height);
        return ExtractSoftBits(image, bitCount, quarterTurns: 0, originalSize: null, permutations: null, buffer);
    }

    private double[] ExtractSoftBits(SKBitmap image, int bitCount, int quarterTurns, SKSizeI? originalSize, byte[]? permutations, LowBuffer reusableLow, double[]? reusableBlockValues = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }
        if (bitCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), "水印位数必须大于零。");
        }

        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new ArgumentException("载体图片的宽度和高度必须大于零。", nameof(image));
        }

        var blockRows = reusableLow.Rows / BlockSize;
        var blockColumns = reusableLow.Columns / BlockSize;
        var blockCount = checked(blockRows * blockColumns);
        ValidateCapacity(bitCount, blockCount, originalSize?.Width ?? image.Width, originalSize?.Height ?? image.Height);
        TransformImage(image, quarterTurns, originalSize, reusableLow);
        var low = reusableLow.Channels;

        var sums = new double[bitCount];
        var counts = new int[bitCount];
        permutations ??= CreatePermutations(blockCount);
        var blockValues = reusableBlockValues ?? new double[blockCount];

        for (var channel = 0; channel < low.Length; channel++)
        {
            var channelLow = low[channel];
            Parallel.For(0, blockRows, blockY =>
            {
                Span<double> block = stackalloc double[16];
                Span<double> dct = stackalloc double[16];
                Span<double> singularValues = stackalloc double[4];
                Span<double> vectors = stackalloc double[16];
                for (var blockX = 0; blockX < blockColumns; blockX++)
                {
                    var blockIndex = blockY * blockColumns + blockX;
                    ReadBlock(channelLow, blockY, blockX, block);
                    Dct(block, dct);
                    ApplyPermutation(dct, permutations.AsSpan(blockIndex * 16, 16));
                    SingularValueDecomposition(dct, singularValues, vectors);
                    // 两个奇异值分别投票；第二奇异值启用时权重为四分之一。
                    var value = (singularValues[0] % _d1 > _d1 / 2) ? 1.0 : 0.0;
                    if (_d2 > 0)
                    {
                        var second = singularValues[1] % _d2 > _d2 / 2 ? 1.0 : 0.0;
                        value = (value * 3 + second) / 4;
                    }

                    blockValues[blockIndex] = value;
                }
            });

            // 同一水印位循环嵌入多个块和三个通道，累积后取平均以减轻损坏影响。
            for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                var messageIndex = blockIndex % bitCount;
                sums[messageIndex] += blockValues[blockIndex];
                counts[messageIndex]++;
            }
        }

        for (var i = 0; i < bitCount; i++)
        {
            sums[i] /= counts[i];
        }

        // 重建嵌入时的置乱顺序，恢复水印位的原始位置。
        var randomOrder = Enumerable.Range(0, bitCount).ToArray();
        Shuffle(randomOrder, CreateRandom(WatermarkPassword));
        var decrypted = new double[bitCount];
        for (var i = 0; i < bitCount; i++)
        {
            decrypted[randomOrder[i]] = sums[i];
        }

        return decrypted;
    }

    /// <summary>按当前图片尺寸和方向提取字节形式的二进制水印。</summary>
    /// <param name="image">包含水印的图片；本方法不自动恢复旋转或缩放。</param>
    /// <param name="byteCount">嵌入时的水印字节数，必须大于零。</param>
    /// <returns>按原水印顺序排列的字节数组，每 8 个水印位还原为一个字节。</returns>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> 不大于零。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或不足以容纳指定的水印字节数。</exception>
    /// <exception cref="OverflowException">水印字节数转换为位数时溢出。</exception>
    public byte[] ExtractBits(SKBitmap image, int byteCount)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }

        if (byteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), "水印字节数必须大于零。");
        }

        return DecodeBytes(ClassifyBits(ExtractSoftBits(image, checked(byteCount * 8))));
    }

    /// <summary>从可能经过缩放或直角旋转的图片中提取字节形式的二进制水印。</summary>
    /// <param name="image">包含水印的图片。</param>
    /// <param name="byteCount">嵌入时的水印字节数，必须大于零。</param>
    /// <param name="originalSize">嵌入水印前载体图片的宽高，均须大于零。</param>
    /// <returns>按原水印顺序排列的字节数组。</returns>
    /// <remarks>本方法按原始尺寸最近邻采样，并启发式地尝试四个直角方向。缩放和压缩可能破坏水印；提取结果没有校验码。</remarks>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">字节数或原始图片尺寸无效。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或原始尺寸对应的容量不足。</exception>
    /// <exception cref="OverflowException">水印字节数转换为位数时溢出。</exception>
    public byte[] ExtractBits(SKBitmap image, int byteCount, SKSizeI originalSize)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }

        if (byteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), "水印字节数必须大于零。");
        }

        if (originalSize.Width <= 0 || originalSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalSize), "载体图片的原始宽度和高度必须大于零。");
        }

        var values = ExtractBestSoftBits(image, checked(byteCount * 8), originalSize);
        return DecodeBytes(ClassifyBits(values));
    }

    private static bool[] ClassifyBits(double[] values)
    {
        var minimum = values.Min();
        var maximum = values.Max();
        if (minimum >= 0.5 || maximum < 0.5 || Math.Abs(maximum - minimum) < Epsilon)
        {
            return values.Select(value => value >= 0.5).ToArray();
        }

        // 将软判决值聚成两组，并以组中心的中点作为分类阈值。
        var (low, high) = KMeansThreshold(values);
        var threshold = (low + high) / 2;
        return values.Select(value => value > threshold).ToArray();
    }

    /// <summary>从图片中提取 UTF-8 文本水印，自动尝试四个直角方向。</summary>
    /// <param name="image">包含水印的图片；此重载不恢复缩放。</param>
    /// <param name="utf8ByteCount">原文本的 UTF-8 字节数，而不是字符数。</param>
    /// <returns>提取出的文本。</returns>
    /// <remarks>方向选择是启发式的，无法保证检测出错误结果。</remarks>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="utf8ByteCount"/> 不大于零。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或容量不足。</exception>
    /// <exception cref="OverflowException">字节数转换为位数时溢出。</exception>
    /// <exception cref="InvalidOperationException">提取结果不是有效的 UTF-8。</exception>
    public string ExtractText(SKBitmap image, int utf8ByteCount) => ExtractTextCore(image, utf8ByteCount, originalSize: null);

    /// <summary>从可能经过缩放或直角旋转的图片中提取 UTF-8 文本水印。</summary>
    /// <param name="image">包含水印的图片。</param>
    /// <param name="utf8ByteCount">原文本的 UTF-8 字节数，而不是字符数。</param>
    /// <param name="originalSize">嵌入水印前载体图片的宽高，均须大于零。</param>
    /// <returns>提取出的文本。</returns>
    /// <remarks>本方法按原始尺寸最近邻采样，并启发式地尝试四个直角方向；缩放和压缩造成的信息丢失可能导致提取失败或结果错误。</remarks>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">字节数或原始图片尺寸无效。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或原始尺寸对应的容量不足。</exception>
    /// <exception cref="OverflowException">字节数转换为位数时溢出。</exception>
    /// <exception cref="InvalidOperationException">提取结果不是有效的 UTF-8。</exception>
    public string ExtractText(SKBitmap image, int utf8ByteCount, SKSizeI originalSize)
    {
        if (originalSize.Width <= 0 || originalSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalSize), "载体图片的原始宽度和高度必须大于零。");
        }

        return ExtractTextCore(image, utf8ByteCount, originalSize);
    }

    private string ExtractTextCore(SKBitmap image, int utf8ByteCount, SKSizeI? originalSize)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }
        if (utf8ByteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(utf8ByteCount), "水印文本的 UTF-8 字节数必须大于零。");
        }

        var values = ExtractBestSoftBits(image, checked(utf8ByteCount * 8), originalSize);
        var bytes = DecodeBytes(ClassifyBits(values));
        try
        {
            return new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidOperationException("提取的水印不是有效的 UTF-8 文本，请检查图片、密码和水印长度。", exception);
        }
    }

    private double[] ExtractBestSoftBits(SKBitmap image, int bitCount, SKSizeI? originalSize)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }
        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new ArgumentException("载体图片的宽度和高度必须大于零。", nameof(image));
        }

        var width = originalSize?.Width ?? image.Width;
        var height = originalSize?.Height ?? image.Height;
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("载体图片的宽度和高度必须大于零。", nameof(image));
        }

        var blockCount = GetBlockCount(width, height);
        ValidateCapacity(bitCount, blockCount, width, height);
        var permutations = CreatePermutations(blockCount);
        using var reusableLow = new LowBuffer(width, height);
        using var rotatedLow = originalSize.HasValue || width == height ? null : new LowBuffer(height, width);
        var blockValues = new double[blockCount];
        // 正确方向的水印信号通常离随机投票的中点更远；该评分只是启发式判断。
        var bestValues = ExtractSoftBits(image, bitCount, quarterTurns: 0, originalSize, permutations, reusableLow, blockValues);
        var bestScore = bestValues.Average(value => (value - 0.5) * (value - 0.5));
        for (var quarterTurns = 1; quarterTurns < 4; quarterTurns++)
        {
            var values = ExtractSoftBits(image, bitCount, quarterTurns, originalSize, permutations, quarterTurns % 2 == 0 ? reusableLow : rotatedLow ?? reusableLow, blockValues);
            var score = values.Average(value => (value - 0.5) * (value - 0.5));
            if (score > bestScore)
            {
                bestScore = score;
                bestValues = values;
            }
        }

        return bestValues;
    }

    /// <summary>按当前图片尺寸和方向提取二值图片水印。</summary>
    /// <param name="image">包含水印的图片；本重载不自动恢复旋转或缩放。</param>
    /// <param name="width">嵌入时水印图片的宽度。</param>
    /// <param name="height">嵌入时水印图片的高度。</param>
    /// <returns>黑白水印图片；调用者负责释放。</returns>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">水印宽度或高度不大于零。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或容量不足。</exception>
    /// <exception cref="OverflowException">水印像素数超出可表示的范围。</exception>
    public SKBitmap ExtractImage(SKBitmap image, int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "水印图片的宽度必须大于零。");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "水印图片的高度必须大于零。");
        }

        return CreateWatermarkImage(ClassifyBits(ExtractSoftBits(image, checked(width * height))), width, height);
    }

    /// <summary>从可能经过缩放或直角旋转的图片中提取二值图片水印。</summary>
    /// <param name="image">包含水印的图片。</param>
    /// <param name="width">嵌入时水印图片的宽度。</param>
    /// <param name="height">嵌入时水印图片的高度。</param>
    /// <param name="originalSize">嵌入水印前载体图片的宽高，而不是水印图片的尺寸。</param>
    /// <returns>黑白水印图片；调用者负责释放。</returns>
    /// <remarks>本方法按原始尺寸最近邻采样，并启发式地尝试四个直角方向。缩放和压缩可能导致噪点或方向识别错误，水印本身不提供校验。</remarks>
    /// <exception cref="ArgumentNullException">图片为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">水印或原始图片的尺寸不大于零。</exception>
    /// <exception cref="ArgumentException">图片尺寸无效或原始尺寸对应的容量不足。</exception>
    /// <exception cref="OverflowException">水印像素数超出可表示的范围。</exception>
    public SKBitmap ExtractImage(SKBitmap image, int width, int height, SKSizeI originalSize)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "水印图片的宽度必须大于零。");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "水印图片的高度必须大于零。");
        }

        if (originalSize.Width <= 0 || originalSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalSize), "载体图片的原始宽度和高度必须大于零。");
        }

        var values = ExtractBestSoftBits(image, checked(width * height), originalSize);
        return CreateWatermarkImage(ClassifyBits(values), width, height);
    }

    private static SKBitmap CreateWatermarkImage(bool[] bits, int width, int height)
    {
        var result = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        for (var i = 0; i < bits.Length; i++)
        {
            var value = bits[i] ? (byte) 255 : (byte) 0;
            result.SetPixel(i % width, i / width, new SKColor(value, value, value));
        }

        return result;
    }

    private SKBitmap Process(SKBitmap image, bool[] watermark)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image), "载体图片不能为空。");
        }
        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new ArgumentException("载体图片的宽度和高度必须大于零。", nameof(image));
        }

        ValidateCapacity(watermark.Length, GetBlockCount(image.Width, image.Height), image.Width, image.Height);
        using var buffer = new LowBuffer(image.Width, image.Height);
        TransformImage(image, 0, originalSize: null, buffer);
        var low = buffer.Channels;
        var blockRows = buffer.Rows / BlockSize;
        var blockColumns = buffer.Columns / BlockSize;
        var blockCount = checked(blockRows * blockColumns);
        ValidateCapacity(watermark.Length, blockCount, image.Width, image.Height);
        var permutations = CreatePermutations(blockCount);

        // 每个 YUV 通道的 Haar 低频部分分为 4×4 块，水印位按块循环嵌入。
        for (var channel = 0; channel < low.Length; channel++)
        {
            var channelLow = low[channel];
            Parallel.For(0, blockRows, blockY =>
            {
                Span<double> block = stackalloc double[16];
                Span<double> dct = stackalloc double[16];
                for (var blockX = 0; blockX < blockColumns; blockX++)
                {
                    var blockIndex = blockY * blockColumns + blockX;
                    ReadBlock(channelLow, blockY, blockX, block);
                    Dct(block, dct);
                    ApplyPermutation(dct, permutations.AsSpan(blockIndex * 16, 16));

                    EmbedSingularValues(dct, watermark[blockIndex % watermark.Length]);
                    InversePermutation(dct, permutations.AsSpan(blockIndex * 16, 16));
                    InverseDct(dct, block);
                    WriteBlockDelta(channelLow, blockY, blockX, block);
                }
            });

            // 未凑满整块的低频系数没有被修改，重建时其增量应为零。
            for (var y = 0; y < channelLow.Length; y++)
            {
                var start = y < blockRows * BlockSize ? blockColumns * BlockSize : 0;
                Array.Clear(channelLow[y], start, buffer.Columns - start);
            }
        }

        return ReconstructImage(image, low);
    }

    private static bool[] EncodeBytes(byte[] bytes)
    {
        var bits = new bool[checked(bytes.Length * 8)];
        for (var i = 0; i < bytes.Length; i++)
        {
            for (var bit = 0; bit < 8; bit++)
            {
                bits[i * 8 + bit] = ((bytes[i] >> (7 - bit)) & 1) != 0;
            }
        }

        return bits;
    }

    private static byte[] DecodeBytes(bool[] bits)
    {
        var bytes = new byte[bits.Length / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            byte value = 0;
            for (var bit = 0; bit < 8; bit++)
            {
                value = (byte) ((value << 1) | (bits[i * 8 + bit] ? 1 : 0));
            }

            bytes[i] = value;
        }

        return bytes;
    }

    private static void TransformImage(SKBitmap image, int quarterTurns, SKSizeI? originalSize, LowBuffer buffer)
    {
        var orientedWidth = quarterTurns % 2 == 0 ? image.Width : image.Height;
        var orientedHeight = quarterTurns % 2 == 0 ? image.Height : image.Width;
        var width = originalSize?.Width ?? orientedWidth;
        var height = originalSize?.Height ?? orientedHeight;
        var rows = buffer.Rows;
        var columns = buffer.Columns;
        // 以像素中心做最近邻反向映射，在读取时同时处理旋转和尺寸恢复。
        var sampleXs = new int[width];
        var sampleYs = new int[height];
        for (var x = 0; x < width; x++)
        {
            sampleXs[x] = (int) (((2L * x + 1) * orientedWidth) / (2L * width));
        }

        for (var y = 0; y < height; y++)
        {
            sampleYs[y] = (int) (((2L * y + 1) * orientedHeight) / (2L * height));
        }

        var low = buffer.Channels;
        var fastPixels = CanAccessPixelsDirectly(image);
        var rowBytes = image.RowBytes;
        var rgba = image.ColorType == SKColorType.Rgba8888;
        var imageWidth = image.Width;
        var imageHeight = image.Height;
        Parallel.For(0, rows, y =>
        {
            ReadOnlySpan<byte> pixels = fastPixels ? image.GetPixelSpan() : ReadOnlySpan<byte>.Empty;
            for (var x = 0; x < columns; x++)
            {
                var a = ReadYuv(image, pixels, rowBytes, rgba, quarterTurns, imageWidth, imageHeight, sampleXs, sampleYs, x * 2, y * 2);
                var b = ReadYuv(image, pixels, rowBytes, rgba, quarterTurns, imageWidth, imageHeight, sampleXs, sampleYs, x * 2 + 1, y * 2);
                var c = ReadYuv(image, pixels, rowBytes, rgba, quarterTurns, imageWidth, imageHeight, sampleXs, sampleYs, x * 2, y * 2 + 1);
                var d = ReadYuv(image, pixels, rowBytes, rgba, quarterTurns, imageWidth, imageHeight, sampleXs, sampleYs, x * 2 + 1, y * 2 + 1);
                // Haar 低频系数为 2×2 像素之和的一半；越界像素按零填充。
                for (var channel = 0; channel < 3; channel++)
                {
                    var av = channel == 0 ? a.Y : channel == 1 ? a.U : a.V;
                    var bv = channel == 0 ? b.Y : channel == 1 ? b.U : b.V;
                    var cv = channel == 0 ? c.Y : channel == 1 ? c.U : c.V;
                    var dv = channel == 0 ? d.Y : channel == 1 ? d.U : d.V;
                    low[channel][y][x] = (av + bv + cv + dv) / 2;
                }
            }
        });
    }

    // 通道行缓冲区由池复用；归还时清零，并在构造失败时释放已租用的数组。
    private sealed class LowBuffer : IDisposable
    {
        private bool _disposed;

        public LowBuffer(int width, int height)
        {
            Rows = height / 2 + height % 2;
            Columns = width / 2 + width % 2;
            Channels = new double[3][][];
            try
            {
                for (var channel = 0; channel < Channels.Length; channel++)
                {
                    Channels[channel] = new double[Rows][];
                    for (var y = 0; y < Rows; y++)
                    {
                        Channels[channel][y] = ArrayPool<double>.Shared.Rent(Columns);
                    }
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public int Rows { get; }
        public int Columns { get; }
        public double[][][] Channels { get; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var row in Channels.OfType<double[][]>().SelectMany(channel => channel.OfType<double[]>()))
            {
                ArrayPool<double>.Shared.Return(row, clearArray: true);
            }
        }
    }

    // 仅直接读取已知字节布局且未预乘的像素；其他格式交给 SkiaSharp 转换。
    private static bool CanAccessPixelsDirectly(SKBitmap image) => (image.ColorType == SKColorType.Rgba8888 || image.ColorType == SKColorType.Bgra8888) && (image.AlphaType == SKAlphaType.Opaque || image.AlphaType == SKAlphaType.Unpremul) && image.GetPixels() != IntPtr.Zero;

    private static (double Y, double U, double V) ReadYuv(SKBitmap image, ReadOnlySpan<byte> pixels, int rowBytes, bool rgba, int quarterTurns, int imageWidth, int imageHeight, int[] sampleXs, int[] sampleYs, int x, int y)
    {
        if (x >= sampleXs.Length || y >= sampleYs.Length)
        {
            return (0, 0, 0);
        }

        var sampleX = sampleXs[x];
        var sampleY = sampleYs[y];
        var (sourceX, sourceY) = quarterTurns switch
        {
            0 => (sampleX, sampleY),
            1 => (sampleY, imageHeight - 1 - sampleX),
            2 => (imageWidth - 1 - sampleX, imageHeight - 1 - sampleY),
            3 => (imageWidth - 1 - sampleY, sampleX),
            _ => throw new ArgumentOutOfRangeException(nameof(quarterTurns), "旋转方向必须为 0、1、2 或 3。")
        };
        if (pixels.IsEmpty)
        {
            return ToYuv(image.GetPixel(sourceX, sourceY));
        }

        var index = sourceY * rowBytes + sourceX * 4;
        var color = rgba ? new SKColor(pixels[index], pixels[index + 1], pixels[index + 2]) : new SKColor(pixels[index + 2], pixels[index + 1], pixels[index]);
        return ToYuv(color);
    }

    private static SKBitmap ReconstructImage(SKBitmap source, double[][][] lowDelta)
    {
        var result = new SKBitmap(source.Width, source.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var fastPixels = CanAccessPixelsDirectly(source);
        var sourceRowBytes = source.RowBytes;
        var resultRowBytes = result.RowBytes;
        var opaque = source.AlphaType == SKAlphaType.Opaque;
        var width = source.Width;
        var rgba = source.ColorType == SKColorType.Rgba8888;
        try
        {
            Parallel.For(0, source.Height, y =>
            {
                var originalPixels = fastPixels ? source.GetPixelSpan() : ReadOnlySpan<byte>.Empty;
                var output = result.GetPixelSpan();
                var blockY = y / 2;
                for (var x = 0; x < width; x++)
                {
                    var blockX = x / 2;
                    var sourceIndex = y * sourceRowBytes + x * 4;
                    SKColor color;
                    if (fastPixels)
                    {
                        var alpha = opaque ? (byte) 255 : originalPixels[sourceIndex + 3];
                        color = rgba ? new SKColor(originalPixels[sourceIndex], originalPixels[sourceIndex + 1], originalPixels[sourceIndex + 2], alpha) : new SKColor(originalPixels[sourceIndex + 2], originalPixels[sourceIndex + 1], originalPixels[sourceIndex], alpha);
                    }
                    else
                    {
                        color = source.GetPixel(x, y);
                    }

                    var (luma, u, v) = ToYuv(color);
                    // 仅保存低频系数的增量；一个增量的一半作用于对应的四个像素。
                    var (red, green, blue) = FromYuv(luma + lowDelta[0][blockY][blockX] / 2, u + lowDelta[1][blockY][blockX] / 2, v + lowDelta[2][blockY][blockX] / 2);
                    var index = y * resultRowBytes + x * 4;
                    output[index] = ToByte(red);
                    output[index + 1] = ToByte(green);
                    output[index + 2] = ToByte(blue);
                    output[index + 3] = color.Alpha;
                }
            });
            result.NotifyPixelsChanged();
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    private void EmbedSingularValues(Span<double> matrix, bool bit)
    {
        Span<double> singularValues = stackalloc double[4];
        Span<double> vectors = stackalloc double[16];
        SingularValueDecomposition(matrix, singularValues, vectors);
        Span<double> target = stackalloc double[2];
        // 将奇异值量化到区间的 1/4 或 3/4 处，分别表示水印位 0 和 1。
        target[0] = (Math.Floor(singularValues[0] / _d1) + 0.25 + (bit ? 0.5 : 0)) * _d1;
        target[1] = _d2 > 0 ? (Math.Floor(singularValues[1] / _d2) + 0.25 + (bit ? 0.5 : 0)) * _d2 : singularValues[1];

        Span<double> left = stackalloc double[4];
        Span<double> firstLeft = stackalloc double[4];
        for (var k = 0; k < 2; k++)
        {
            var delta = target[k] - singularValues[k];
            if (Math.Abs(delta) < Epsilon)
            {
                continue;
            }

            left.Clear();
            if (singularValues[k] > Epsilon)
            {
                GetLeftVector(matrix, vectors, singularValues[k], k, left);
            }
            else
            {
                // 奇异值为零时无法除以它求左奇异向量，改为构造正交向量。
                if (k == 1)
                {
                    if (singularValues[0] > Epsilon)
                    {
                        GetLeftVector(matrix, vectors, singularValues[0], 0, firstLeft);
                    }
                    else
                    {
                        FindOrthogonalVector(ReadOnlySpan<double>.Empty, firstLeft);
                    }
                }

                FindOrthogonalVector(k == 0 ? ReadOnlySpan<double>.Empty : firstLeft, left);
            }

            for (var row = 0; row < BlockSize; row++)
            {
                for (var column = 0; column < BlockSize; column++)
                {
                    matrix[row * BlockSize + column] += delta * left[row] * vectors[column * BlockSize + k];
                }
            }
        }
    }

    private static void GetLeftVector(ReadOnlySpan<double> matrix, ReadOnlySpan<double> vectors, double singularValue, int index, Span<double> result)
    {
        for (var row = 0; row < BlockSize; row++)
        {
            var sum = 0.0;
            for (var column = 0; column < BlockSize; column++)
            {
                sum += matrix[row * BlockSize + column] * vectors[column * BlockSize + index];
            }

            result[row] = sum / singularValue;
        }
    }

    private static void FindOrthogonalVector(ReadOnlySpan<double> existing, Span<double> candidate)
    {
        for (var index = 0; index < BlockSize; index++)
        {
            candidate.Clear();
            candidate[index] = 1;
            if (!existing.IsEmpty)
            {
                var dot = existing[index];
                for (var i = 0; i < BlockSize; i++)
                {
                    candidate[i] -= dot * existing[i];
                }
            }

            var lengthSquared = 0.0;
            for (var i = 0; i < BlockSize; i++)
            {
                lengthSquared += candidate[i] * candidate[i];
            }

            var length = Math.Sqrt(lengthSquared);
            if (length > Epsilon)
            {
                for (var i = 0; i < BlockSize; i++)
                {
                    candidate[i] /= length;
                }

                return;
            }
        }

        throw new InvalidOperationException("无法构造正交奇异向量。");
    }

    private static void SingularValueDecomposition(ReadOnlySpan<double> matrix, Span<double> singularValues, Span<double> sortedVectors)
    {
        Span<double> gram = stackalloc double[16];
        Span<double> vectors = stackalloc double[16];
        gram.Clear();
        vectors.Clear();
        for (var row = 0; row < BlockSize; row++)
        {
            vectors[row * BlockSize + row] = 1;
            for (var column = 0; column < BlockSize; column++)
            {
                for (var k = 0; k < BlockSize; k++)
                {
                    gram[row * BlockSize + column] += matrix[k * BlockSize + row] * matrix[k * BlockSize + column];
                }
            }
        }

        // 对 AᵀA 做 Jacobi 特征分解，特征值的平方根即为奇异值。
        for (var iteration = 0; iteration < 100; iteration++)
        {
            var p = 0;
            var q = 1;
            var largest = 0.0;
            for (var row = 0; row < BlockSize; row++)
            {
                for (var column = row + 1; column < BlockSize; column++)
                {
                    var value = Math.Abs(gram[row * BlockSize + column]);
                    if (value > largest)
                    {
                        largest = value;
                        p = row;
                        q = column;
                    }
                }
            }

            if (largest < 1e-12)
            {
                break;
            }

            var app = gram[p * BlockSize + p];
            var aqq = gram[q * BlockSize + q];
            var apq = gram[p * BlockSize + q];
            var tau = (aqq - app) / (2 * apq);
            var t = tau >= 0 ? 1 / (tau + Math.Sqrt(1 + tau * tau)) : -1 / (-tau + Math.Sqrt(1 + tau * tau));
            var cosine = 1 / Math.Sqrt(1 + t * t);
            var sine = t * cosine;

            for (var k = 0; k < BlockSize; k++)
            {
                if (k == p || k == q)
                {
                    continue;
                }

                var gkp = gram[k * BlockSize + p];
                var gkq = gram[k * BlockSize + q];
                gram[k * BlockSize + p] = gram[p * BlockSize + k] = cosine * gkp - sine * gkq;
                gram[k * BlockSize + q] = gram[q * BlockSize + k] = sine * gkp + cosine * gkq;
            }

            gram[p * BlockSize + p] = app - t * apq;
            gram[q * BlockSize + q] = aqq + t * apq;
            gram[p * BlockSize + q] = gram[q * BlockSize + p] = 0;

            for (var k = 0; k < BlockSize; k++)
            {
                var vkp = vectors[k * BlockSize + p];
                var vkq = vectors[k * BlockSize + q];
                vectors[k * BlockSize + p] = cosine * vkp - sine * vkq;
                vectors[k * BlockSize + q] = sine * vkp + cosine * vkq;
            }
        }

        Span<int> order = [0, 1, 2, 3];
        for (var i = 1; i < BlockSize; i++)
        {
            var index = order[i];
            var value = Math.Max(0, gram[index * BlockSize + index]);
            var j = i;
            while (j > 0 && Math.Max(0, gram[order[j - 1] * BlockSize + order[j - 1]]) < value)
            {
                order[j] = order[j - 1];
                j--;
            }

            order[j] = index;
        }

        for (var column = 0; column < BlockSize; column++)
        {
            singularValues[column] = Math.Sqrt(Math.Max(0, gram[order[column] * BlockSize + order[column]]));
            for (var row = 0; row < BlockSize; row++)
            {
                sortedVectors[row * BlockSize + column] = vectors[row * BlockSize + order[column]];
            }
        }
    }

    private static double[] CreateDctBasis()
    {
        var basis = new double[BlockSize * BlockSize];
        for (var u = 0; u < BlockSize; u++)
        {
            var alpha = u == 0 ? Math.Sqrt(1.0 / BlockSize) : Math.Sqrt(2.0 / BlockSize);
            for (var x = 0; x < BlockSize; x++)
            {
                basis[u * BlockSize + x] = alpha * Math.Cos((2 * x + 1) * u * Math.PI / (2 * BlockSize));
            }
        }

        return basis;
    }

    // 二维 DCT 可分离为两个一维变换，复用预计算的正交基。
    private static void Dct(ReadOnlySpan<double> block, Span<double> result)
    {
        Span<double> intermediate = stackalloc double[16];
        for (var u = 0; u < BlockSize; u++)
        {
            for (var x = 0; x < BlockSize; x++)
            {
                var sum = 0.0;
                for (var y = 0; y < BlockSize; y++)
                {
                    sum += DctBasis[u * BlockSize + y] * block[y * BlockSize + x];
                }

                intermediate[u * BlockSize + x] = sum;
            }

            for (var v = 0; v < BlockSize; v++)
            {
                var sum = 0.0;
                for (var x = 0; x < BlockSize; x++)
                {
                    sum += intermediate[u * BlockSize + x] * DctBasis[v * BlockSize + x];
                }

                result[u * BlockSize + v] = sum;
            }
        }
    }

    private static void InverseDct(ReadOnlySpan<double> coefficients, Span<double> result)
    {
        Span<double> intermediate = stackalloc double[16];
        for (var y = 0; y < BlockSize; y++)
        {
            for (var v = 0; v < BlockSize; v++)
            {
                var sum = 0.0;
                for (var u = 0; u < BlockSize; u++)
                {
                    sum += DctBasis[u * BlockSize + y] * coefficients[u * BlockSize + v];
                }

                intermediate[y * BlockSize + v] = sum;
            }

            for (var x = 0; x < BlockSize; x++)
            {
                var sum = 0.0;
                for (var v = 0; v < BlockSize; v++)
                {
                    sum += intermediate[y * BlockSize + v] * DctBasis[v * BlockSize + x];
                }

                result[y * BlockSize + x] = sum;
            }
        }
    }

    private static void ApplyPermutation(Span<double> block, ReadOnlySpan<byte> permutation)
    {
        Span<double> copy = stackalloc double[16];
        block.CopyTo(copy);
        for (var i = 0; i < copy.Length; i++)
        {
            block[i] = copy[permutation[i]];
        }
    }

    private static void InversePermutation(Span<double> block, ReadOnlySpan<byte> permutation)
    {
        Span<double> copy = stackalloc double[16];
        block.CopyTo(copy);
        for (var i = 0; i < copy.Length; i++)
        {
            block[permutation[i]] = copy[i];
        }
    }

    private static void ReadBlock(double[][] matrix, int blockY, int blockX, Span<double> block)
    {
        for (var y = 0; y < BlockSize; y++)
        {
            for (var x = 0; x < BlockSize; x++)
            {
                block[y * BlockSize + x] = matrix[blockY * BlockSize + y][blockX * BlockSize + x];
            }
        }
    }

    private static void WriteBlockDelta(double[][] matrix, int blockY, int blockX, ReadOnlySpan<double> block)
    {
        for (var y = 0; y < BlockSize; y++)
        {
            for (var x = 0; x < BlockSize; x++)
            {
                var row = matrix[blockY * BlockSize + y];
                var column = blockX * BlockSize + x;
                row[column] = block[y * BlockSize + x] - row[column];
            }
        }
    }

    private static void ValidateCapacity(int bitCount, int blockCount, int width, int height)
    {
        if (blockCount == 0 || bitCount > blockCount)
        {
            throw new ArgumentException($"水印需要 {bitCount} 位，但尺寸为 {width}x{height} 的图片最多只能容纳 {blockCount} 位。");
        }
    }

    private static int GetBlockCount(int width, int height) => checked((int) ((((long) height + 1) / 2 / BlockSize) * (((long) width + 1) / 2 / BlockSize)));

    private static (double Low, double High) KMeansThreshold(double[] values)
    {
        var low = values.Min();
        var high = values.Max();
        if (Math.Abs(high - low) < Epsilon)
        {
            return (low, high);
        }

        for (var iteration = 0; iteration < 300; iteration++)
        {
            var threshold = (low + high) / 2;
            var lowValues = values.Where(value => value <= threshold).ToArray();
            var highValues = values.Where(value => value > threshold).ToArray();
            if (lowValues.Length == 0 || highValues.Length == 0)
            {
                break;
            }

            var nextLow = lowValues.Average();
            var nextHigh = highValues.Average();
            if (Math.Abs((nextLow + nextHigh) / 2 - threshold) < 1e-6)
            {
                low = nextLow;
                high = nextHigh;
                break;
            }

            low = nextLow;
            high = nextHigh;
        }

        return (low, high);
    }

    // 每个块使用由图片密码和块序号确定的置乱，嵌入和提取须生成相同顺序。
    private byte[] CreatePermutations(int blockCount)
    {
        var permutations = new byte[checked(blockCount * BlockSize * BlockSize)];
        for (var block = 0; block < blockCount; block++)
        {
            var offset = block * BlockSize * BlockSize;
            for (var i = 0; i < BlockSize * BlockSize; i++)
            {
                permutations[offset + i] = (byte) i;
            }

            var seed = unchecked((uint) ImagePassword * 0x9E3779B97F4A7C15UL + (uint) block);
            var random = new SplitMix64(seed);
            for (var i = BlockSize * BlockSize - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (permutations[offset + i], permutations[offset + j]) = (permutations[offset + j], permutations[offset + i]);
            }
        }

        return permutations;
    }

    private static void Shuffle<T>(T[] values, SplitMix64 random)
    {
        for (var i = values.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    private static SplitMix64 CreateRandom(int seed) => new(unchecked((uint) seed));

    private static (double Y, double U, double V) ToYuv(SKColor color)
    {
        var red = (double) color.Red;
        var green = (double) color.Green;
        var blue = (double) color.Blue;
        var y = 0.299 * red + 0.587 * green + 0.114 * blue;
        return (y, -0.14713 * red - 0.28886 * green + 0.436 * blue + 128, 0.615 * red - 0.51499 * green - 0.10001 * blue + 128);
    }

    private static (double Red, double Green, double Blue) FromYuv(double y, double u, double v)
    {
        u -= 128;
        v -= 128;
        return (y + 1.13983 * v, y - 0.39465 * u - 0.58060 * v, y + 2.03211 * u);
    }

    private static byte ToByte(double value) => (byte) Clamp((int) Math.Round(value), 0, 255);
    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
            return min;
        return value > max ? max : value;
    }
    private struct SplitMix64(ulong seed)
    {
        private ulong _state = seed;

        public int Next(int exclusiveMaximum)
        {
            _state += 0x9E3779B97F4A7C15UL;
            var value = _state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            value ^= value >> 31;
            return (int) (value % (uint) exclusiveMaximum);
        }
    }
}
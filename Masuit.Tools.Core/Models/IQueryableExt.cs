using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.Tools.Models
{
    public static partial class IQueryableExt
    {
        /// <summary>
        /// 生成分页集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page">当前页</param>
        /// <param name="size">页大小</param>
        /// <returns></returns>
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IOrderedQueryable<T> query, int page, int size)
        {
            var totalCount = await query.CountAsync();
            if (page * size > totalCount)
            {
                page = (int)Math.Ceiling(totalCount / (size * 1.0));
            }

            if (page <= 0)
            {
                page = 1;
            }

            var list = await query.Skip(size * (page - 1)).Take(size).ToListAsync();
            return new PagedList<T>(list, page, size, totalCount);
        }
    }
}

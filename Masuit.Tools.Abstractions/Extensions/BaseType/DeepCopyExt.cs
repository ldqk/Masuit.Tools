using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public static class DeepCopyExt
    {
        private static readonly object IsStructTypeToDeepCopyDictionaryLocker = new object();
        private static Dictionary<Type, bool> _isStructTypeToDeepCopyDictionary = new Dictionary<Type, bool>();
        private static readonly object CompiledCopyFunctionsDictionaryLocker = new object();
        private static Dictionary<Type, Func<object, Dictionary<object, object>, object>> _compiledCopyFunctionsDictionary = new Dictionary<Type, Func<object, Dictionary<object, object>, object>>();
        private static readonly Type ObjectType = typeof(object);
        private static readonly Type ObjectDictionaryType = typeof(Dictionary<object, object>);

        /// <summary>
        /// 深克隆
        /// </summary>
        /// <param name="original">原始对象</param>
        /// <param name="copiedReferencesDict">需要被引用传递的对象(Keys: 原始对象, Values: 副本对象).</param>
        /// <returns></returns>
        public static T DeepClone<T>(this T original, Dictionary<object, object> copiedReferencesDict = null)
        {
            return (T)DeepCopyObj(original, false, copiedReferencesDict ?? new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        private static object DeepCopyObj(object original, bool forceDeepCopy, Dictionary<object, object> copiedReferencesDict)
        {
            if (original == null)
            {
                return null;
            }

            var type = original.GetType();

            if (IsDelegate(type))
            {
                return null;
            }

            if (!forceDeepCopy && !IsTypeToDeepCopy(type))
            {
                return original;
            }

            if (copiedReferencesDict.TryGetValue(original, out var alreadyCopiedObject))
            {
                return alreadyCopiedObject;
            }

            if (type == ObjectType)
            {
                return new object();
            }

            var compiledCopyFunction = GetOrCreateCompiledLambdaCopyFunction(type);
            object copy = compiledCopyFunction(original, copiedReferencesDict);
            return copy;
        }

        private static Func<object, Dictionary<object, object>, object> GetOrCreateCompiledLambdaCopyFunction(Type type)
        {
            if (!_compiledCopyFunctionsDictionary.TryGetValue(type, out var compiledCopyFunction))
            {
                lock (CompiledCopyFunctionsDictionaryLocker)
                {
                    if (!_compiledCopyFunctionsDictionary.TryGetValue(type, out compiledCopyFunction))
                    {
                        var uncompiledCopyFunction = CreateCompiledLambdaCopyFunctionForType(type);
                        compiledCopyFunction = uncompiledCopyFunction.Compile();
                        var dictionaryCopy = _compiledCopyFunctionsDictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
                        dictionaryCopy.Add(type, compiledCopyFunction);
                        _compiledCopyFunctionsDictionary = dictionaryCopy;
                    }
                }
            }

            return compiledCopyFunction;
        }

        private static Expression<Func<object, Dictionary<object, object>, object>> CreateCompiledLambdaCopyFunctionForType(Type type)
        {
            InitializeExpressions(type, out var inputParameter, out var inputDictionary, out var outputVariable, out var boxingVariable, out var endLabel, out var variables, out var expressions);
            IfNullThenReturnNullExpression(inputParameter, endLabel, expressions);
            MemberwiseCloneInputToOutputExpression(type, inputParameter, outputVariable, expressions);
            if (IsClassOtherThanString(type))
            {
                StoreReferencesIntoDictionaryExpression(inputParameter, inputDictionary, outputVariable, expressions);
            }

            FieldsCopyExpressions(type, inputParameter, inputDictionary, outputVariable, boxingVariable, expressions);
            if (IsArray(type) && IsTypeToDeepCopy(type.GetElementType()))
            {
                CreateArrayCopyLoopExpression(type, inputParameter, inputDictionary, outputVariable, variables, expressions);
            }

            var lambda = CombineAllIntoLambdaFunctionExpression(inputParameter, inputDictionary, outputVariable, endLabel, variables, expressions);
            return lambda;
        }

        private static void InitializeExpressions(Type type, out ParameterExpression inputParameter, out ParameterExpression inputDictionary, out ParameterExpression outputVariable, out ParameterExpression boxingVariable, out LabelTarget endLabel, out List<ParameterExpression> variables, out List<Expression> expressions)
        {
            inputParameter = Expression.Parameter(ObjectType);
            inputDictionary = Expression.Parameter(ObjectDictionaryType);
            outputVariable = Expression.Variable(type);
            boxingVariable = Expression.Variable(ObjectType);
            endLabel = Expression.Label();
            variables = new List<ParameterExpression>();
            expressions = new List<Expression>();
            variables.Add(outputVariable);
            variables.Add(boxingVariable);
        }

        private static void IfNullThenReturnNullExpression(ParameterExpression inputParameter, LabelTarget endLabel, List<Expression> expressions)
        {
            var ifNullThenReturnNullExpression = Expression.IfThen(Expression.Equal(inputParameter, Expression.Constant(null, ObjectType)), Expression.Return(endLabel));
            expressions.Add(ifNullThenReturnNullExpression);
        }

        private static void MemberwiseCloneInputToOutputExpression(Type type, ParameterExpression inputParameter, ParameterExpression outputVariable, List<Expression> expressions)
        {
            var memberwiseCloneMethod = ObjectType.GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            var memberwiseCloneInputExpression = Expression.Assign(outputVariable, Expression.Convert(Expression.Call(inputParameter, memberwiseCloneMethod), type));
            expressions.Add(memberwiseCloneInputExpression);
        }

        private static void StoreReferencesIntoDictionaryExpression(ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, List<Expression> expressions)
        {
            var storeReferencesExpression = Expression.Assign(Expression.Property(inputDictionary, ObjectDictionaryType.GetProperty("Item"), inputParameter), Expression.Convert(outputVariable, ObjectType));
            expressions.Add(storeReferencesExpression);
        }

        private static Expression<Func<object, Dictionary<object, object>, object>> CombineAllIntoLambdaFunctionExpression(ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, LabelTarget endLabel, List<ParameterExpression> variables, List<Expression> expressions)
        {
            expressions.Add(Expression.Label(endLabel));
            expressions.Add(Expression.Convert(outputVariable, ObjectType));
            var finalBody = Expression.Block(variables, expressions);
            var lambda = Expression.Lambda<Func<object, Dictionary<object, object>, object>>(finalBody, inputParameter, inputDictionary);
            return lambda;
        }

        private static void CreateArrayCopyLoopExpression(Type type, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, List<ParameterExpression> variables, List<Expression> expressions)
        {
            var rank = type.GetArrayRank();
            var indices = GenerateIndices(rank);
            variables.AddRange(indices);
            var elementType = type.GetElementType();
            var assignExpression = ArrayFieldToArrayFieldAssignExpression(inputParameter, inputDictionary, outputVariable, elementType, type, indices);
            Expression forExpression = assignExpression;
            for (int dimension = 0; dimension < rank; dimension++)
            {
                var indexVariable = indices[dimension];
                forExpression = LoopIntoLoopExpression(inputParameter, indexVariable, forExpression, dimension);
            }

            expressions.Add(forExpression);
        }

        private static List<ParameterExpression> GenerateIndices(int arrayRank)
        {
            var indices = new List<ParameterExpression>();
            for (int i = 0; i < arrayRank; i++)
            {
                var indexVariable = Expression.Variable(typeof(int));
                indices.Add(indexVariable);
            }

            return indices;
        }

        private static BinaryExpression ArrayFieldToArrayFieldAssignExpression(ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, Type elementType, Type arrayType, List<ParameterExpression> indices)
        {
            var indexTo = Expression.ArrayAccess(outputVariable, indices);
            var indexFrom = Expression.ArrayIndex(Expression.Convert(inputParameter, arrayType), indices);
            var forceDeepCopy = elementType != ObjectType;
            var rightSide = Expression.Convert(Expression.Call(DeepCopyByExpressionTreeObjMethod, Expression.Convert(indexFrom, ObjectType), Expression.Constant(forceDeepCopy, typeof(bool)), inputDictionary), elementType);
            var assignExpression = Expression.Assign(indexTo, rightSide);
            return assignExpression;
        }

        private static BlockExpression LoopIntoLoopExpression(ParameterExpression inputParameter, ParameterExpression indexVariable, Expression loopToEncapsulate, int dimension)
        {
            var lengthVariable = Expression.Variable(typeof(int));
            var endLabelForThisLoop = Expression.Label();
            var newLoop = Expression.Loop(Expression.Block(new ParameterExpression[0], Expression.IfThen(Expression.GreaterThanOrEqual(indexVariable, lengthVariable), Expression.Break(endLabelForThisLoop)), loopToEncapsulate, Expression.PostIncrementAssign(indexVariable)), endLabelForThisLoop);
            var lengthAssignment = GetLengthForDimensionExpression(lengthVariable, inputParameter, dimension);
            var indexAssignment = Expression.Assign(indexVariable, Expression.Constant(0));
            return Expression.Block(new[]
            {
                lengthVariable
            }, lengthAssignment, indexAssignment, newLoop);
        }

        private static BinaryExpression GetLengthForDimensionExpression(ParameterExpression lengthVariable, ParameterExpression inputParameter, int i)
        {
            var getLengthMethod = typeof(Array).GetMethod("GetLength", BindingFlags.Public | BindingFlags.Instance);
            var dimensionConstant = Expression.Constant(i);
            return Expression.Assign(lengthVariable, Expression.Call(Expression.Convert(inputParameter, typeof(Array)), getLengthMethod, new[]
            {
                dimensionConstant
            }));
        }

        private static void FieldsCopyExpressions(Type type, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, ParameterExpression boxingVariable, List<Expression> expressions)
        {
            var fields = GetAllRelevantFields(type);
            var readonlyFields = fields.Where(f => f.IsInitOnly).ToList();
            var writableFields = fields.Where(f => !f.IsInitOnly).ToList();
            bool shouldUseBoxing = readonlyFields.Any();
            if (shouldUseBoxing)
            {
                var boxingExpression = Expression.Assign(boxingVariable, Expression.Convert(outputVariable, ObjectType));
                expressions.Add(boxingExpression);
            }

            foreach (var field in readonlyFields)
            {
                if (IsDelegate(field.FieldType))
                {
                    ReadonlyFieldToNullExpression(field, boxingVariable, expressions);
                }
                else
                {
                    ReadonlyFieldCopyExpression(type, field, inputParameter, inputDictionary, boxingVariable, expressions);
                }
            }

            if (shouldUseBoxing)
            {
                var unboxingExpression = Expression.Assign(outputVariable, Expression.Convert(boxingVariable, type));
                expressions.Add(unboxingExpression);
            }

            foreach (var field in writableFields)
            {
                if (IsDelegate(field.FieldType))
                {
                    WritableFieldToNullExpression(field, outputVariable, expressions);
                }
                else
                {
                    WritableFieldCopyExpression(type, field, inputParameter, inputDictionary, outputVariable, expressions);
                }
            }
        }

        private static FieldInfo[] GetAllRelevantFields(Type type, bool forceAllFields = false)
        {
            var fieldsList = new List<FieldInfo>();
            var typeCache = type;
            while (typeCache != null)
            {
                fieldsList.AddRange(typeCache.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(field => forceAllFields || IsTypeToDeepCopy(field.FieldType)));
                typeCache = typeCache.BaseType;
            }

            return fieldsList.ToArray();
        }

        private static FieldInfo[] GetAllFields(Type type)
        {
            return GetAllRelevantFields(type, true);
        }

        private static readonly Type FieldInfoType = typeof(FieldInfo);

        private static readonly MethodInfo SetValueMethod = FieldInfoType.GetMethod("SetValue", new[]
        {
            ObjectType,
            ObjectType
        });

        private static void ReadonlyFieldToNullExpression(FieldInfo field, ParameterExpression boxingVariable, List<Expression> expressions)
        {
            var fieldToNullExpression = Expression.Call(Expression.Constant(field), SetValueMethod, boxingVariable, Expression.Constant(null, field.FieldType));
            expressions.Add(fieldToNullExpression);
        }

        private static readonly Type ThisType = typeof(DeepCopyExt);
        private static readonly MethodInfo DeepCopyByExpressionTreeObjMethod = ThisType.GetMethod("DeepCopyByExpressionTreeObj", BindingFlags.NonPublic | BindingFlags.Static);

        private static void ReadonlyFieldCopyExpression(Type type, FieldInfo field, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression boxingVariable, List<Expression> expressions)
        {
            var fieldFrom = Expression.Field(Expression.Convert(inputParameter, type), field);
            var forceDeepCopy = field.FieldType != ObjectType;
            var fieldDeepCopyExpression = Expression.Call(Expression.Constant(field, FieldInfoType), SetValueMethod, boxingVariable, Expression.Call(DeepCopyByExpressionTreeObjMethod, Expression.Convert(fieldFrom, ObjectType), Expression.Constant(forceDeepCopy, typeof(bool)), inputDictionary));
            expressions.Add(fieldDeepCopyExpression);
        }

        private static void WritableFieldToNullExpression(FieldInfo field, ParameterExpression outputVariable, List<Expression> expressions)
        {
            var fieldTo = Expression.Field(outputVariable, field);
            var fieldToNullExpression = Expression.Assign(fieldTo, Expression.Constant(null, field.FieldType));
            expressions.Add(fieldToNullExpression);
        }

        private static void WritableFieldCopyExpression(Type type, FieldInfo field, ParameterExpression inputParameter, ParameterExpression inputDictionary, ParameterExpression outputVariable, List<Expression> expressions)
        {
            var fieldFrom = Expression.Field(Expression.Convert(inputParameter, type), field);
            var fieldType = field.FieldType;
            var fieldTo = Expression.Field(outputVariable, field);
            var forceDeepCopy = field.FieldType != ObjectType;
            var fieldDeepCopyExpression = Expression.Assign(fieldTo, Expression.Convert(Expression.Call(DeepCopyByExpressionTreeObjMethod, Expression.Convert(fieldFrom, ObjectType), Expression.Constant(forceDeepCopy, typeof(bool)), inputDictionary), fieldType));
            expressions.Add(fieldDeepCopyExpression);
        }

        private static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        private static bool IsDelegate(Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        private static bool IsTypeToDeepCopy(Type type)
        {
            return IsClassOtherThanString(type) || IsStructWhichNeedsDeepCopy(type);
        }

        private static bool IsClassOtherThanString(Type type)
        {
            return !type.IsValueType && type != typeof(string);
        }

        private static bool IsStructWhichNeedsDeepCopy(Type type)
        {
            if (!_isStructTypeToDeepCopyDictionary.TryGetValue(type, out var isStructTypeToDeepCopy))
            {
                lock (IsStructTypeToDeepCopyDictionaryLocker)
                {
                    if (!_isStructTypeToDeepCopyDictionary.TryGetValue(type, out isStructTypeToDeepCopy))
                    {
                        isStructTypeToDeepCopy = IsStructWhichNeedsDeepCopy_NoDictionaryUsed(type);
                        var newDictionary = _isStructTypeToDeepCopyDictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
                        newDictionary[type] = isStructTypeToDeepCopy;
                        _isStructTypeToDeepCopyDictionary = newDictionary;
                    }
                }
            }

            return isStructTypeToDeepCopy;
        }

        private static bool IsStructWhichNeedsDeepCopy_NoDictionaryUsed(Type type)
        {
            return IsStructOtherThanBasicValueTypes(type) && HasInItsHierarchyFieldsWithClasses(type);
        }

        private static bool IsStructOtherThanBasicValueTypes(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum && type != typeof(decimal);
        }

        private static bool HasInItsHierarchyFieldsWithClasses(Type type, HashSet<Type> alreadyCheckedTypes = null)
        {
            alreadyCheckedTypes ??= new HashSet<Type>();
            alreadyCheckedTypes.Add(type);
            var allFields = GetAllFields(type);
            var allFieldTypes = allFields.Select(f => f.FieldType).Distinct().ToList();
            var hasFieldsWithClasses = allFieldTypes.Any(IsClassOtherThanString);
            if (hasFieldsWithClasses)
            {
                return true;
            }

            var notBasicStructsTypes = allFieldTypes.Where(IsStructOtherThanBasicValueTypes).ToList();
            var typesToCheck = notBasicStructsTypes.Where(t => !alreadyCheckedTypes.Contains(t)).ToList();
            return typesToCheck.Any(typeToCheck => HasInItsHierarchyFieldsWithClasses(typeToCheck, alreadyCheckedTypes));
        }

        public class ReferenceEqualityComparer : EqualityComparer<object>
        {
            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public override int GetHashCode(object obj)
            {
                if (obj == null) return 0;
                return obj.GetHashCode();
            }
        }
    }
}
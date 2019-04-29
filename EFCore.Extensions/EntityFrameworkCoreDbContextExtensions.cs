using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class EntityFrameworkCoreDbContextExtensions
    {
        private static readonly MethodInfo DbContextSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set));
        private static readonly MethodInfo DbContextQueryMethod = typeof(DbContext).GetMethod(nameof(DbContext.Query));

        public static IQueryable Set(this DbContext ctx, string entityTypeName)
        {
            var entityType = ctx.Model.FindEntityType(entityTypeName);
            if (entityType == null && !entityType.IsQueryType)
                throw new KeyNotFoundException($"未找到名为{entityTypeName}的实体类型");

            return (IQueryable)DbContextSetMethod.MakeGenericMethod(entityType.ClrType).Invoke(ctx, null);
        }

        public static IQueryable Query(this DbContext ctx, string entityTypeName)
        {
            var entityType = ctx.Model.FindEntityType(entityTypeName);
            if (entityType == null && entityType.IsQueryType)
                throw new KeyNotFoundException($"未找到名为{entityTypeName}的查询类型");

            return (IQueryable)DbContextQueryMethod.MakeGenericMethod(entityType.ClrType).Invoke(ctx, null);
        }

        public static object Find(this DbContext context, string entityTypeName, params object[] keys)
        {
            var entityType = context.Model.FindEntityType(entityTypeName);
            if (entityType == null)
                throw new KeyNotFoundException($"未找到名为{entityTypeName}的实体类型");

            return context.Find(entityType.ClrType, keys);
        }

        public static Task<object> FindAsync(this DbContext context, string entityTypeName, params object[] keys) =>
            context.FindAsync(entityTypeName, keys, default);

        public static Task<object> FindAsync(this DbContext context, string entityTypeName, object[] keys, CancellationToken cancellationToken)
        {
            var entityType = context.Model.FindEntityType(entityTypeName);
            if (entityType == null)
                throw new KeyNotFoundException($"未找到名为{entityTypeName}的实体类型");

            return context.FindAsync(entityType.ClrType, keys, cancellationToken);
        }
    }
}

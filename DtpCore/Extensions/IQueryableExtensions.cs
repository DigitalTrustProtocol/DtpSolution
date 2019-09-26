using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace DtpCore.Extensions
{
    //public static class IQueryableExtensions
    //{
    //    private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

    //    private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");

    //    private static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");

    //    private static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");

    //    private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

    //    private static readonly PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

    //    public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
    //    {
    //        if (!(query is EntityQueryable<TEntity>) && !(query is InternalDbSet<TEntity>))
    //        {
    //            throw new ArgumentException("Invalid query");
    //        }

    //        var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
    //        var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue(queryCompiler);
    //        var parser = (IQueryParser)CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
    //        var queryModel = parser.GetParsedQuery(query.Expression);
    //        var database = DataBaseField.GetValue(queryCompiler);
    //        var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
    //        var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
    //        var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
    //        modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
    //        var sql = modelVisitor.Queries.First().ToString();
    //        return sql;
    //    }


    //    public static object Filter(this IQueryable items, List<String> columnNames, String sort, String order, Int32 limit, Int32 offset)
    //    {
    //        // where clause is set, count total records
    //        var count = items.Count();

    //        // Skip requires sorting, so make sure there is always sorting
    //        var sortExpression = "";

    //        if (sort != null && sort.Length > 0)
    //            sortExpression += String.Format("{0} {1}", sort, order);

    //        if (string.IsNullOrWhiteSpace(sortExpression))
    //            sortExpression = "ID";

    //        // show all records if limit is not set
    //        if (limit == 0)
    //            limit = count;

    //        // Prepare json structure
    //        var result = new
    //        {
    //            total = count,
    //            rows = items.OrderBy(sortExpression).Skip(offset).Take(limit).Select("new (" + String.Join(",", columnNames) + ")")
    //        };
    //        return result;

    //        //return JsonConvert.SerializeObject(result, Formatting.None, new JsonSerializerSettings() { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
    //    }


    //    // needs System.Linq.Dynamic.Core
    //    public static IQueryable Search(this IQueryable items, String search, List<String> columnNames)
    //    {
    //        // Apply filtering to all visible column names
    //        if (search != null && search.Length > 0)
    //        {
    //            StringBuilder sb = new StringBuilder();

    //            // create dynamic Linq expression
    //            foreach (String fieldName in columnNames)
    //                sb.AppendFormat("({0} == null ? false : {0}.ToString().IndexOf(@0, @1) >=0) or {1}", fieldName, Environment.NewLine);

    //            String searchExpression = sb.ToString();
    //            // remove last "or" occurrence
    //            searchExpression = searchExpression.Substring(0, searchExpression.LastIndexOf("or"));

    //            // Apply filtering, 
    //            items = items.Where(searchExpression, search, StringComparison.OrdinalIgnoreCase);
    //        }

    //        return items;
    //    }

    //}
}

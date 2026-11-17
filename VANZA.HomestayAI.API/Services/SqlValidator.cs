
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace VANZA.HomestayAI.API.Services
{
    public class SqlValidator
    {
        public bool IsValidSelect(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            var parser = new TSql160Parser(true);

            using var reader = new StringReader(sql);

            var fragment = parser.Parse(
                reader,
                out IList<ParseError> errors);

            if (errors.Count > 0)
                return false;

            if (fragment is not TSqlScript script)
                return false;

            if (script.Batches.Count != 1)
                return false;

            if (script.Batches[0].Statements.Count != 1)
                return false;

            return script.Batches[0].Statements[0]
                is SelectStatement;
        }
    }
}

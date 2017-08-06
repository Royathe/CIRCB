namespace CIRCBot
{
    public class ParamInt
    {

        public int ParamId { get; set; }

        public string ParamAttName { get; set; }

        public int ParamCode { get; set; }

        public string ParamText { get; set; }

        public int ParamValue { get; set; }

        public void Update()
        {
            Sql.Query.UpdateParamInt(this);
        }
    }
}

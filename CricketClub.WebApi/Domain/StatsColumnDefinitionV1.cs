namespace CricketClub.WebApi.Domain
{
    public class StatsColumnDefinitionV1
    {
        public string headerName { get; private set; }
        public string field { get; private set; }
        public string cellRenderer { get; private set; }

        public StatsColumnDefinitionV1(string headerName, string field, string renderer)
        {
            this.headerName = headerName;
            this.field = field;
            this.cellRenderer = renderer;
        }
        
        public StatsColumnDefinitionV1(string headerName, string field)
        {
            this.headerName = headerName;
            this.field = field;
        }
    }
}

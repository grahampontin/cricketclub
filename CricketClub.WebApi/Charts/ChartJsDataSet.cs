namespace CricketClub.WebApi.Charts
{
    public class ChartJsDataSet
    {
        public ChartJsDataSet()
        {
            data = new List<object>();
        }

        public bool spanGaps { get; set; }
    
        public string label { get; set; }
        public string type { get; set; }
        public List<object> data { get; set; }
        public object backgroundColor { get; set; }
        public object borderColor { get; set; }
        public int? borderWidth { get; set; }
    }
}
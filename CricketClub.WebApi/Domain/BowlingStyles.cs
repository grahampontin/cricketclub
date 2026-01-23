namespace CricketClub.WebApi.Domain
{
    public class BowlingStyles
    {
        public static List<Tuple<String, String>> Styles = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("RF", "Right-arm fast"),
            new Tuple<string, string>("RFM", "Right-arm fast-medium"),
            new Tuple<string, string>("RMF", "Right-arm medium-fast"),
            new Tuple<string, string>("RM", "Right-arm medium"),
            new Tuple<string, string>("RMS", "Right-arm medium-slow"),
            new Tuple<string, string>("RSM", "Right-arm slow-medium"),
            new Tuple<string, string>("RS", "Right-arm slow"),
            new Tuple<string, string>("LF", "Left-arm fast"),
            new Tuple<string, string>("LFM", "Left-arm fast-medium"),
            new Tuple<string, string>("LMF", "Left-arm medium-fast"),
            new Tuple<string, string>("LM", "Left-arm medium"),
            new Tuple<string, string>("LMS", "Left-arm medium-slow"),
            new Tuple<string, string>("LSM", "Left-arm slow-medium"),
            new Tuple<string, string>("LS", "Left-arm slow"),
            new Tuple<string, string>("OB","Off break"),
            new Tuple<string, string>("LB","Leg break"),
            new Tuple<string, string>("LBG","Leg break googly"),
            new Tuple<string, string>("SLA","Slow left-arm orthodox"),
            new Tuple<string, string>("SLW","Slow left-arm wrist spin"),
            new Tuple<string, string>("LAG","Left-arm googly")
        };

        public static IEnumerable<string> Abbreviations
        {
            get
            {
                return Styles.Select(s => s.Item1);
            } 
        }

    }
}
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Knowledge.Model
{
    public class Question
    {
        public Question()
        {
        }

        public string id { get; set; }
        public string title { get; set; }

        public List<long> assignedAnswers { get; set; }
        public int source { get; set; }
        public int status { get; set; }
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }

        public void AddQuestion(QuestionData questionData)
        {
            throw new NotImplementedException();
        }
    }
}


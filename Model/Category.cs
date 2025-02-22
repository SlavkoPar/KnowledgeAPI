using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Knowledge.Model
{
    
    public interface ICategoryData
    {
		string id { get; set; }
		string title { get; set; }
        int kind { get; set; }
        //IList<string>? variations { get; set; }
        //IList<ICategoryData>? categories { get; set; }
        //IList<IQuestionData>? questions { get; set; }
    }


    public class Category
    {
        public Category()
        {
        }

        public string id { get; set; }
        public string title { get; set; }
        public int kind { get; set; }
        public string parentCategory { get; set; }
        public string[] words { get; set; }
        public int level { get; set; }
        public string[] variations { get; set; }
        public int numOfQuestions { get; set; }
        public bool hasSubCategories { get; set; }
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }

        public void AddCategory(CategoryData categoryData)
        {
            throw new NotImplementedException();
        }
    }


}


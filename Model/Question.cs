using System.Drawing.Printing;
using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;


namespace Knowledge.Model
{
    public class Question
    {
        public string type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string title { get; set; }
        public string? parentCategory { get; set; }
        // public IList<string> words { get; set; }
        public List<long> assignedAnswers { get; set; }
        public int source { get; set; }
        public int status { get; set; }

        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }

        public static Db? Db { get; set; } = null;

        private static readonly string containerId = "Questions";
        public static Container? container { get; set; } = null;

        public static string? partitionKey { get; set; } = null;
        public Question()
        {
        }

        public Question(IConfiguration configuration)
        {
            Question.Db = new Db(configuration);
        }

        public Question(Db Db)
        {
            Question.Db = Db;
        }


        public Question(QuestionData questionData)
        {
            this.type = "question";
            this.Id = Guid.NewGuid().ToString(); // questionData.id;
            this.PartitionKey = questionData.PartitionKey!;
            this.title = questionData.title;
            //this.words =
            //    categoryData.title
            //        .ToLower()
            //        .Replace("?", "")
            //        .Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
            //        .Where(w => w.Length > 1)
            //        .ToList();
            this.parentCategory = questionData.parentCategory;
            this.assignedAnswers = [];
            this.created = new WhoWhen("Admin");
            this.modified = null;
            this.archived = null;
        }

        public async Task<HttpStatusCode> CheckDuplicate(QuestionData questionData)
        {
            var sqlQuery = $"SELECT * FROM c WHERE c.type = 'question' AND c.title = '{questionData.title}'";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
            FeedIterator<Question> queryResultSetIterator =
                Question.container!.GetItemQueryIterator<Question>(queryDefinition);
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count == 0)
                {
                    throw new CosmosException("Question Title already exists", HttpStatusCode.NotFound, 0, "0", 0);
                }
            }
            return HttpStatusCode.OK;
        }

        public async Task AddQuestion(QuestionData questionData)
        {
            if (Question.container == null)
            {
                Question.container = await Question.Db!.GetContainer(Question.containerId);
            }

            if (questionData.parentCategory == null)
            {
                Question.partitionKey = questionData.id;
                questionData.PartitionKey = Question.partitionKey;
            }

            // Create a question object 
            Question question = new Question(questionData);
            try
            {
                // Read the item to see if it exists.  
                HttpStatusCode statusCode = await CheckDuplicate(questionData);
                Console.WriteLine("Item in database with id: {0} already exists\n", statusCode);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Question> aResponse =
                    await Question.container.CreateItemAsync<Question>(
                        question,
                        new PartitionKey(question.PartitionKey)
                    );

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task<Question> GetQuestion(string partitionKey, string id)
        {
            if (Question.container == null)
            {
                Question.container = await Question.Db!.GetContainer(Question.containerId);
            }

            Question question = null;
            try
            {
                // Read the item to see if it exists.  
                question = await Question.container.ReadItemAsync<Question>(
                    id,
                    new PartitionKey(partitionKey)
                );
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return question;
        }

        public static async Task<QuestionsMore> GetQuestions(string parentCategory, int startCursor, int pageSize, string includeQuestionId)
        {
            if (Question.container == null)
            {
                Question.container = await Question.Db!.GetContainer(Question.containerId);
            }
            List<Question> questions = new List<Question>();
            bool hasMore = false;
            try
            {
                // OR c.parentCategory = ''
                var sqlQuery = $"SELECT * FROM c WHERE c.type = 'question' AND IS_NULL(c.archived) AND c.parentCategory = '{parentCategory}' ORDER BY c.title ";
                sqlQuery += includeQuestionId == null
                    ? $"OFFSET {startCursor} LIMIT {pageSize}"
                    : $"OFFSET {startCursor} LIMIT 9999";

                int n = 0;
                bool included = false;

                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Question> queryResultSetIterator = container.GetItemQueryIterator<Question>(queryDefinition);
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Question question in currentResultSet)
                    {
                        if (includeQuestionId != "null" && question.Id == includeQuestionId)
                        {
                            included = true;
                        }
                        questions.Add(question);
                        n++;
                        if (n >= pageSize && (includeQuestionId != "null" ? included : true))
                        {
                            hasMore = true;
                            return new QuestionsMore(questions, hasMore);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionsMore(questions, hasMore);
        }


    }
}

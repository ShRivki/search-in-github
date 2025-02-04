using Octokit;


namespace Service
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;

        public GitHubService(string personalAccessToken)
        {
            _client = new GitHubClient(new ProductHeaderValue("CVSite"))
            {
                Credentials = new Credentials(personalAccessToken)
            };
        }

        public async Task<List<RepositoryInfo>> GetPortfolioAsync(string userName)
        {
            var repositories = await _client.Repository.GetAllForUser(userName);

            var portfolio = new List<RepositoryInfo>();
            foreach (var repo in repositories)
            {
                var languages = await _client.Repository.GetAllLanguages(repo.Id);
                portfolio.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    LastCommit = repo.PushedAt,
                    Stars = repo.StargazersCount,
                    PullRequests = await GetPullRequestCount(repo.Id),
                    Url = repo.HtmlUrl,
                    Languages = languages.Select(language => language.Name)
                });
            }
            return portfolio;
        }

        //public async Task<List<Repository>> SearchRepositoriesAsync(string repoName, string language, string userName)
        //{
        //    var request = new SearchRepositoriesRequest(repoName);

        //    if (!string.IsNullOrEmpty(language))
        //    {
        //        var languageEnum = Enum.Parse<Language>(language);
        //        request.Language = languageEnum;
        //    }

        //    if (!string.IsNullOrEmpty(userName))
        //    {
        //        request.User = userName;
        //    }

        //    var result = await _client.Search.SearchRepo(request);
        //    return result.Items.ToList();
        //}



        public async Task<List<RepositoryInfo>> SearchRepositoriesAsync(string repoName = "", string language = "", string userName = "")
        {
            // יצירת בקשה לחיפוש רפוזיטוריז
            var request = new SearchRepositoriesRequest(repoName);

            // אם הוזנה שפה, נוסיף אותה לחיפוש
            if (!string.IsNullOrEmpty(language))
            {
                try
                {
                    var languageEnum = Enum.Parse<Language>(language, true); // המרה לשפת תכנות
                    request.Language = languageEnum;
                }
                catch
                {
                    throw new ArgumentException("Invalid language provided.");
                }
            }

            // אם הוזן שם משתמש, נוסיף אותו לחיפוש
            if (!string.IsNullOrEmpty(userName))
            {
                request.User = userName;
            }

            // ביצוע החיפוש
            var result = await _client.Search.SearchRepo(request);

            // המרת התוצאות לאובייקטים של RepositoryInfo
            var repositoriesInfo = new List<RepositoryInfo>();

            foreach (var repo in result.Items)
            {
                var languages = await _client.Repository.GetAllLanguages(repo.Owner.Login, repo.Name); // קבלת כל השפות של הרפוזיטורי
                var pullRequests = await _client.PullRequest.GetAllForRepository(repo.Id); // קבלת כל ה-Pull Requests הפתוחים

                repositoriesInfo.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    LastCommit = repo.PushedAt, // זמן האחרון שבו בוצע Commit
                    Stars = repo.StargazersCount, // מספר כוכבים
                    PullRequests = pullRequests.Count, // מספר ה-Pull Requests הפתוחים
                    Url = repo.HtmlUrl, // כתובת ה-URL של הרפוזיטורי
                    Languages = languages.Select(language => language.Name) // רשימת השפות (מפתחות של השפות)
                });
            }

            return repositoriesInfo;
        }
        private async Task<int> GetPullRequestCount(long repoId)
        {
            var pullRequests = await _client.PullRequest.GetAllForRepository(repoId);
            return pullRequests.Count;
        }
    }


    public class RepositoryInfo
        {
            public string Name { get; set; }
            public DateTimeOffset? LastCommit { get; set; }
            public int Stars { get; set; }
            public int PullRequests { get; set; }
            public string Url { get; set; }
            public IEnumerable<string> Languages { get; set; }
        }


    
}

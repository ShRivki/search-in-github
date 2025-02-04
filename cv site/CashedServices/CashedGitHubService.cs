using Service;

namespace cv_site.CashedServices
{
    public class CashedGitHubService : IGitHubService
    {
        private readonly IGitHubService _gitHubService;
        public CashedGitHubService(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }
        public Task<List<RepositoryInfo>> GetPortfolioAsync(string userName)
        {
            return _gitHubService.GetPortfolioAsync(userName);
        }

        public Task<List<RepositoryInfo>> SearchRepositoriesAsync(string repoName = "", string language = "", string userName = "")
        {
            return _gitHubService.SearchRepositoriesAsync();
        }
    }
}


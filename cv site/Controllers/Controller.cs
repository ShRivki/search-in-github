using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Service;

[ApiController]
[Route("api/[controller]")]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "Portfolio";

    public GitHubController(IGitHubService gitHubService, IMemoryCache cache)
    {
        _gitHubService = gitHubService;
        _cache = cache;
    }

    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio(string userName)
    {
        if (!_cache.TryGetValue(_cacheKey, out List<RepositoryInfo> portfolio))
        {
            portfolio = await _gitHubService.GetPortfolioAsync(userName);

            _cache.Set(_cacheKey, portfolio, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        return Ok(portfolio);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRepositories(string repoName="", string language = "", string userName = "")
    {
        var result = await _gitHubService.SearchRepositoriesAsync(repoName, language, userName);
        return Ok(result);
    }
}

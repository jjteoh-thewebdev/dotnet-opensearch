using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;


namespace SkycarApi.Contrrollers
{
    [ApiController]
    [Route("api/skycars")]
    public class SkycarsController : ControllerBase
    {
        private readonly IOpenSearchClient _openSearchClient;
        private readonly string _skycarIndex = "skycars";
        private readonly Random _randomizer = new Random();

        public SkycarsController(
            IOpenSearchClient openSearchClient
        )
        {
            _openSearchClient = openSearchClient;
        }

        // GET /
        [HttpGet]
        public async Task<IActionResult> GetSkycars(
            [FromQuery] string? skycar = null,
            [FromQuery] DateTime? start_date = null,
            [FromQuery] DateTime? end_date = null,
            [FromQuery] int limit = 20
        )
        {
            var query = new List<QueryContainer>();

            // add filter based on the optional parameters
            if (!string.IsNullOrEmpty(skycar))
            {
                query.Add(new MatchQuery
                {
                    Field = "skycar",
                    Query = skycar
                });
            }

            if (start_date.HasValue || end_date.HasValue)
            {
                var dateRangeQuery = new DateRangeQuery
                {
                    Field = "created",

                };

                if (start_date.HasValue)
                {
                    dateRangeQuery.GreaterThanOrEqualTo = start_date.Value;
                }

                if (end_date.HasValue)
                {
                    dateRangeQuery.LessThanOrEqualTo = end_date.Value;
                }

                query.Add(dateRangeQuery);
            }

            // query from opensearch
            var searchRequest = new SearchRequest(_skycarIndex)
            {
                Size = limit,
                Query = new BoolQuery
                {
                    Must = query
                }
            };

            var response = await _openSearchClient.SearchAsync<object>(searchRequest);

            if (!response.IsValid)
            {
                return BadRequest(response.OriginalException.Message);
            }

            return Ok(response.Documents);
        }

        private Skycar _generateDummyData()
        {
            // Generate a random integer between 1 and 999
            var skycar = _randomizer.Next(1, 1000);

            // Generate a random alphanumeric string of length 20
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var randomString = new string(Enumerable.Repeat(chars, 20)
                .Select(s => s[_randomizer.Next(s.Length)])
                .ToArray());

            // Get the current UTC timestamp in ISO 8601 format
            var now = DateTime.UtcNow.ToString("o");

            // Create the message
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var message = $"[{timestamp}] MSEQ_V3: {randomString}";

            // Return as an anonymous object
            return new Skycar { skycar = skycar, message = message, created = DateTime.UtcNow };
        }


        // POST /seed
        [HttpPost("seed")]
        public async Task<IActionResult> Add([FromBody] SeedRequest seedRequest)
        {
            var i = 0;
            int seed = seedRequest.seed ?? 1;
            while (i < seed)
            {
                var skycar = _generateDummyData();
                var response = await _openSearchClient.IndexAsync(skycar, idx => idx.Index(_skycarIndex));

                if (!response.IsValid)
                {
                    return BadRequest(response.OriginalException.Message);
                }

                i++;
            }

            var countRequest = await _openSearchClient.CountAsync<Skycar>(c => c.Index(_skycarIndex));

            return Ok(new { TotalInDB = countRequest.Count });
        }


        // POST /
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Skycar skycarDto)
        {
            // default created to now if not provided
            if (!skycarDto.created.HasValue)
            {
                skycarDto.created = DateTime.UtcNow;
            }

            var response = await _openSearchClient.IndexAsync(skycarDto, idx => idx.Index(_skycarIndex));

            if (!response.IsValid)
            {
                return BadRequest(response.OriginalException.Message);
            }

            return Ok(new { Result = response.Result.ToString(), Id = response.Id });
        }

    }
}


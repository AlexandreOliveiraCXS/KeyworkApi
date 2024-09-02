
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoviesAPI.Data;
using MoviesAPI.Models;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssessmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssessmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("genre")]
        public async Task<IActionResult> Get([FromQuery] NameGenreRequest request)
        {
            var moviesAssessment = await (from assessment in _context.Assessment
                                          join users in _context.Users on assessment.IdUser equals users.Id
                                          join movie in _context.Movies on assessment.IdMovies equals movie.Id
                                          join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                          join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                          where genre.Name == request.Name
                                          select new
                                          { movie, assessment, users, genre }).ToListAsync();

            if (moviesAssessment == null)
                return NotFound("Assessment not found");

            var GenreArray = new Dictionary<string, returnAssessmentGenre>();

            foreach (var item in moviesAssessment)
            {
                var movieReturn = new returnAssessmentGenre();

                movieReturn.Genre = item.genre.Name;
                movieReturn.Id = item.movie.Id;
                movieReturn.Title = item.movie.Title;
                movieReturn.Year = item.movie.Year;
                movieReturn.Month = item.movie.Month;

                if (GenreArray.ContainsKey(movieReturn.Title))
                {
                    movieReturn = GenreArray[movieReturn.Title];
                    movieReturn.AssessmentRating.Add(item.assessment.Rating);
                    movieReturn.AssessmentComment.Add(item.assessment.Comment);

                    GenreArray[movieReturn.Title] = movieReturn;
                }
                else
                {
                    movieReturn.AssessmentRating.Add(item.assessment.Rating);
                    movieReturn.AssessmentComment.Add(item.assessment.Comment);

                    GenreArray.Add(movieReturn.Title, movieReturn);
                }
            }

            return Ok(GenreArray.Values.ToList());
        }

        [HttpGet]
        [Route("full")]
        public async Task<IActionResult> Get()
        {
            var moviesAssessment = await (from assessment in _context.Assessment
                                          join user in _context.Users on assessment.IdUser equals user.Id
                                          join movie in _context.Movies on assessment.IdMovies equals movie.Id
                                          join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                          join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                          select new
                                          { movie, assessment, genre, user }).ToListAsync();

            if (moviesAssessment == null)
                return NotFound("Assessment not found");

            var returnArray = new Dictionary<string, returnAssessmentFull>();

            foreach (var item in moviesAssessment)
            {
                var assessmentReturn = new returnAssessmentFull();

                assessmentReturn.Genre = item.genre.Name;
                assessmentReturn.Year = item.movie.Year;

                if (returnArray.ContainsKey(assessmentReturn.Key))
                {
                    assessmentReturn = returnArray[assessmentReturn.Key];
                    assessmentReturn.Assessments.Add(new AssessmentReturn
                    {
                        Comment = item.assessment.Comment,
                        Rating = item.assessment.Rating,
                        Title = item.movie.Title,
                        User = item.user.Name
                    });

                    returnArray[assessmentReturn.Key] = assessmentReturn;
                }
                else
                {
                    assessmentReturn.Assessments.Add(new AssessmentReturn
                    {
                        Comment = item.assessment.Comment,
                        Rating = item.assessment.Rating,
                        Title = item.movie.Title,
                        User = item.user.Name
                    });

                    returnArray.Add(assessmentReturn.Key, assessmentReturn);
                }
            }

            return Ok(returnArray.Values.ToList());
        }

        [HttpGet]
        [Route("fullPagination")]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var moviesAssessment = await (from assessment in _context.Assessment
                                          join user in _context.Users on assessment.IdUser equals user.Id
                                          join movie in _context.Movies on assessment.IdMovies equals movie.Id
                                          join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                          join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                          select new
                                          { movie, assessment, genre, user }

                                          ).Skip((pageNumber - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            if (moviesAssessment == null)
                return NotFound("Assessment not found");

            var returnArray = new Dictionary<string, returnAssessmentFull>();

            foreach (var item in moviesAssessment)
            {
                var assessmentReturn = new returnAssessmentFull();

                assessmentReturn.Genre = item.genre.Name;
                assessmentReturn.Year = item.movie.Year;

                if (returnArray.ContainsKey(assessmentReturn.Key))
                {
                    assessmentReturn = returnArray[assessmentReturn.Key];
                    assessmentReturn.Assessments.Add(new AssessmentReturn
                    {
                        Comment = item.assessment.Comment,
                        Rating = item.assessment.Rating,
                        Title = item.movie.Title,
                        User = item.user.Name
                    });

                    returnArray[assessmentReturn.Key] = assessmentReturn;
                }
                else
                {
                    assessmentReturn.Assessments.Add(new AssessmentReturn
                    {
                        Comment = item.assessment.Comment,
                        Rating = item.assessment.Rating,
                        Title = item.movie.Title,
                        User = item.user.Name
                    });

                    returnArray.Add(assessmentReturn.Key, assessmentReturn);
                }
            }

            var totalItems = moviesAssessment.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginationMetadata = new
            {
                totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages,
                hasNextPage = pageNumber < totalPages,
                hasPreviousPage = pageNumber > 1
            };
            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            return Ok(returnArray.Values.ToList());
        }

        [HttpGet]
        [Route("movie")]
        public async Task<IActionResult> Get([FromQuery] NameMovieRequest request)
        {
            var moviesAssessment = await (from assessment in _context.Assessment
                                          join users in _context.Users on assessment.IdUser equals users.Id
                                          join movie in _context.Movies on assessment.IdMovies equals movie.Id
                                          where movie.Title == request.Title
                                          select new
                                          { movie, assessment, users }).ToListAsync();

            if (moviesAssessment == null)
                return NotFound("Assessment not found");

            var GenreArray = new Dictionary<string, returnAssessmentMovie>();

            foreach (var item in moviesAssessment)
            {
                var movieReturn = new returnAssessmentMovie();

                movieReturn.Id = item.movie.Id;
                movieReturn.Title = item.movie.Title;
                movieReturn.Year = item.movie.Year;
                movieReturn.Month = item.movie.Month;

                if (GenreArray.ContainsKey(movieReturn.Title))
                {
                    movieReturn = GenreArray[movieReturn.Title];
                    movieReturn.AssessmentRating.Add(item.assessment.Rating);
                    movieReturn.AssessmentComment.Add(item.assessment.Comment);

                    movieReturn.AssessmentsList.Add(new CommentClass
                    {
                        Rating = item.assessment.Rating,
                        Comment = item.assessment.Comment
                    });

                    GenreArray[movieReturn.Title] = movieReturn;
                }
                else
                {
                    movieReturn.AssessmentRating.Add(item.assessment.Rating);
                    movieReturn.AssessmentComment.Add(item.assessment.Comment);

                    movieReturn.AssessmentsList.Add(new CommentClass
                    {
                        Rating = item.assessment.Rating,
                        Comment = item.assessment.Comment
                    });

                    GenreArray.Add(movieReturn.Title, movieReturn);
                }
            }

            return Ok(GenreArray.Values.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AssessmentRequest request)
        {
            if (request == null)
                return BadRequest("Assessment is null.");

            if (request.rating < 0 || request.rating > 5)
                return BadRequest("The grade provided is not correct please enter a grade between 0 and 5.");

            User userFind = null;
            if (!string.IsNullOrEmpty(request.idUser))
                userFind = await _context.Users
                    .Where(m => m.Id.Equals(request.idUser))
                    .FirstOrDefaultAsync();
            else
                userFind = await _context.Users
                    .Where(m => m.Name.Equals(request.nameUser))
                    .FirstOrDefaultAsync();

            var movieFind = await _context.Movies
                    .Where(m => m.Title.Equals(request.title))
                    .FirstOrDefaultAsync();

            if (movieFind == null)
                return NotFound("Movie Not Found.");

            var assessmentFind = await (from assessment in _context.Assessment
                                        join users in _context.Users on assessment.IdUser equals users.Id
                                        join movie in _context.Movies on assessment.IdMovies equals movie.Id
                                        where movie.Title == request.title && users.Id == userFind.Id
                                        select new
                                        { assessment }).FirstOrDefaultAsync();

            if (assessmentFind == null)
            {

                var newObject = new Assessment();

                newObject.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
                newObject.IdMovies = movieFind.Id;
                newObject.IdUser = userFind.Id;
                newObject.Rating = request.rating;
                newObject.Comment = request.comment;
                newObject.CreationTime = DateTime.UtcNow;
                newObject.UpdateTime = DateTime.UtcNow;

                _context.Assessment.Add(newObject);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Post), new { id = newObject.Id }, newObject);
            }
            else
            {
                assessmentFind.assessment.Rating = request.rating;
                assessmentFind.assessment.Comment = request.comment;

                await _context.SaveChangesAsync();

                return AcceptedAtAction(nameof(Post), new { id = assessmentFind.assessment.Id }, assessmentFind.assessment);

            }
        }

        public class AssessmentRequest
        {
            public string idUser { get; set; }
            public string nameUser { get; set; }
            public string title { get; set; }
            public string comment { get; set; }
            public int rating { get; set; }
        }

        public class NameMovieRequest
        {
            public string Title { get; set; }
        }

        public class NameGenreRequest
        {
            public string Name { get; set; }
        }

        private class returnAssessmentMovie : Movie
        {
            public List<double> AssessmentRating { get; set; }
            public List<string> AssessmentComment { get; set; }
            public List<CommentClass> AssessmentsList { get; set; }


            public double ArithmeticMeanRound
            {
                get
                {
                    return Math.Round(ArithmeticMean, MidpointRounding.AwayFromZero);
                }
            }

            public double ArithmeticMean
            {
                get
                {
                    double arithmeticMean = AssessmentsList.Sum((aL) => aL.Rating) / AssessmentsList.Count(); 

                    return arithmeticMean;
                }
            }

            public returnAssessmentMovie()
            {
                AssessmentRating = new List<double>();
                AssessmentComment = new List<string>();
                AssessmentsList = new List<CommentClass>();
            }

        }

        private class CommentClass
        {
            public double Rating { get; set; }
            public string Comment { get; set; }
        }

        private class returnAssessmentGenre : returnAssessmentMovie
        {
            public string Genre { get; set; }
        }

        private class returnStreamings : Streaming
        {
            public List<string> Movies { get; set; }
            public returnStreamings()
            {
                Movies = new List<string>();
            }
        }

        private class returnAssessmentFull
        {
            public string Genre { get; set; }

            public int Year { get; set; }

            public string Key
            {
                get
                {
                    return $"{Genre}|{Year}";
                }
            }

            public double ArithmeticMeanRound
            {
                get
                {
                    return Math.Round(ArithmeticMean, MidpointRounding.AwayFromZero);
                }
            }

            public double ArithmeticMean
            {
                get
                {
                    double arithmeticMean = Assessments.Sum((a) => a.Rating) / Assessments.Count();

                    return arithmeticMean;
                }
            }

            public List<AssessmentReturn> Assessments { get; set; }

            public returnAssessmentFull()
            {
                Assessments = new List<AssessmentReturn>();
            }
        }

        private class AssessmentReturn
        {

            public double Rating { get; set; }
            public string Comment { get; set; }
            public string User { get; set; }
            public string Title { get; set; }
        }
    }
}

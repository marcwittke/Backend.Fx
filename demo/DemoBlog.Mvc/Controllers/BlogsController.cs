namespace DemoBlog.Mvc.Controllers
{
    using System.Linq;
    using Backend.Fx.BuildingBlocks;
    using Domain;
    using Microsoft.AspNetCore.Mvc;

    public class BlogsController : Controller
    {
        private readonly IRepository<Blog> blogsRepository;
        private readonly IRepository<Post> postsRepository;

        public BlogsController(IRepository<Blog> blogsRepository, IRepository<Post> postsRepository)
        {
            this.blogsRepository = blogsRepository;
            this.postsRepository = postsRepository;
        }

        public IActionResult Index()
        {
            return View(blogsRepository.GetAll());
        }

        public IActionResult BlogPosts(int blogId)
        {
            return PartialView(postsRepository.AggregateQueryable.Where(p => p.BlogId == blogId).ToArray());
        }
    }
}

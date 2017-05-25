namespace DemoBlog.Mvc.Controllers
{
    using Backend.Fx.BuildingBlocks;
    using Domain;
    using Microsoft.AspNetCore.Mvc;

    public class BlogsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IRepository<Blog> blogsRepository;

        public BlogsController(IRepository<Blog> blogsRepository)
        {
            this.blogsRepository = blogsRepository;
        }
    }
}

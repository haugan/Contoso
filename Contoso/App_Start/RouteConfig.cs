using System.Web.Mvc;
using System.Web.Routing;

namespace Contoso
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("StudentsIndexRoute", "Students/{*pathInfo}", new { controller = "Student", action = "Index" });
            routes.MapRoute("CoursesIndexRoute", "Courses/{*pathInfo}", new { controller = "Course", action = "Index" });
            routes.MapRoute("EnrollmentsIndexRoute", "Enrollments/{*pathInfo}", new { controller = "Enrollment", action = "Index" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

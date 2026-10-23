using Microsoft.AspNetCore.Identity;

namespace TaskTrack.Data
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Manager", "Member" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Could not create role: {role}");
                    }
                }
            }
        }
    }
}

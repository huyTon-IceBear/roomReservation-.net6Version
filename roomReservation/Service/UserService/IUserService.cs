using Microsoft.AspNetCore.Mvc;

namespace roomReservation.Service.UserService
{
    public interface IUserService
    {
        Task<ActionResult<User>> GetUser();
    }
}

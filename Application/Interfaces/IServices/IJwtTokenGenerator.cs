using Application.Identity;

namespace Application.Interfaces.IServices;

public interface IJwtTokenGenerator
{
    string Generate(User user, IList<string> roles);
}
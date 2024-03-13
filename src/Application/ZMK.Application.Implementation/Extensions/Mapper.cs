using ZMK.Application.Contracts;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Extensions;

public static class Mapper
{
    public static SessionDTO ToDTO(this Session session)
    {
        return new SessionDTO(session.Id, session.UserId);
    }
}
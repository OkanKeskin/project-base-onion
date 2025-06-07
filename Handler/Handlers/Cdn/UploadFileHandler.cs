using Domain.Dtos.Cdn;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Handler.Handlers.Cdn;

public class UploadFileHandler : IRequest<UploadFileResponse>
{
}
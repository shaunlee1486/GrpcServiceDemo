using Grpc.Core;
using GrpcServiceDemo.Model;
using GrpcServiceDemo.Protos;
using Microsoft.EntityFrameworkCore;

namespace GrpcServiceDemo.Services
{
    public class ToDoService : ToDoIt.ToDoItBase
    {
        private readonly AppDbContext _dbContext;

        public ToDoService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Description))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

            var todoItem = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
            };

            await _dbContext.AddAsync(todoItem);
            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new CreateToDoResponse
            {
                Id = todoItem.Id,
            });
        }

        public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var todoItem = await _dbContext.TodoItems.FirstOrDefaultAsync(c => c.Id == request.Id);

            if (todoItem != null)
            {
                return await Task.FromResult(new ReadToDoResponse
                {
                    Id = todoItem.Id,
                    Title = todoItem.Title,
                    Description = todoItem.Description,
                    ToDoStatus = todoItem.TodoStatus,
                });
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id = {request.Id}"));
        }

        public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
        {
            var response = new GetAllResponse();
            var todoItems = await _dbContext.TodoItems.ToListAsync();

            foreach (var todoItem in todoItems)
            {
                response.ToDo.Add(new ReadToDoResponse
                {
                    Id = todoItem.Id,
                    Title = todoItem.Title,
                    Description = todoItem.Description,
                    ToDoStatus = todoItem.TodoStatus,
                });
            }

            return await Task.FromResult(response);
        }

        public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0 || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Description))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

            var todoItem = await _dbContext.TodoItems.FirstOrDefaultAsync(c => c.Id == request.Id);

            if (todoItem is null) throw new RpcException(new Status(StatusCode.NotFound, $"No task with id = {request.Id}"));
            

            todoItem.Title = request.Title;
            todoItem.Description = request.Description;
            todoItem.TodoStatus = request.ToDoStatus;

            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new UpdateToDoResponse { Id = todoItem.Id });
        }

        public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoResponse request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var todoItem = await _dbContext.TodoItems.FirstOrDefaultAsync(c => c.Id == request.Id);

            if (todoItem is null) throw new RpcException(new Status(StatusCode.NotFound, $"No task with id = {request.Id}"));


            _dbContext.Remove(todoItem);

            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new DeleteToDoResponse { Id = todoItem.Id });
        }
    }
}
using FluentAssertions;
using FluentValidation;
using MediatR;
using SkillLearning.Application.Common.Behaviors;

namespace SkillLearning.Tests.UnitTests
{
    public record FakeRequest(string Data) : IRequest<FakeResponse>;
    public record FakeResponse(string Result);

    public class FakeRequestValidator : AbstractValidator<FakeRequest>
    {
        public FakeRequestValidator()
        {
            RuleFor(x => x.Data).NotEmpty();
        }
    }

    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Handle_Should_call_next_delegate_when_no_validators_exist()
        {
            var behavior = new ValidationBehavior<FakeRequest, FakeResponse>([]);
            var request = new FakeRequest("some data");

            bool nextWasCalled = false;
            RequestHandlerDelegate<FakeResponse> next = (ct) =>
            {
                nextWasCalled = true;
                return Task.FromResult(new FakeResponse("Success"));
            };

            await behavior.Handle(request, next, CancellationToken.None);

            nextWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Should_call_next_delegate_when_validation_succeeds()
        {
            var validators = new[] { new FakeRequestValidator() };
            var behavior = new ValidationBehavior<FakeRequest, FakeResponse>(validators);
            var request = new FakeRequest("valid data");

            bool nextWasCalled = false;
            RequestHandlerDelegate<FakeResponse> next = (ct) =>
            {
                nextWasCalled = true;
                return Task.FromResult(new FakeResponse("Success"));
            };

            await behavior.Handle(request, next, CancellationToken.None);

            nextWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Should_throw_ValidationException_when_validation_fails()
        {
            var validators = new[] { new FakeRequestValidator() };
            var behavior = new ValidationBehavior<FakeRequest, FakeResponse>(validators);
            var request = new FakeRequest("");

            bool nextWasCalled = false;
            RequestHandlerDelegate<FakeResponse> next = (ct) =>
            {
                nextWasCalled = true;
                return Task.FromResult(new FakeResponse("Should not be called"));
            };

            Func<Task> action = async () => await behavior.Handle(request, next, CancellationToken.None);

            await action.Should().ThrowAsync<ValidationException>();
            nextWasCalled.Should().BeFalse();
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.Topic;

public sealed record EditTopicDTO(
    [param: Required(ErrorMessage = "Topic id is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "Topic id must be a positive number.")]
    int TopicId,
    [param: Required(ErrorMessage = "Name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    string Name
);

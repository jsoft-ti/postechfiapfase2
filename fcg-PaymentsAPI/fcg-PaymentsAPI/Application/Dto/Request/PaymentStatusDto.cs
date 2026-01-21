using Application.Dto.Enum;
using Application.Dto.Response;

namespace Application.Dto.Request;

public class PaymentStatusDto
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public OrderPlacedEventResponseDto Order{ get; set; }

    public PaymentStatusDto(Guid id, OrderPlacedEventResponseDto order)
    {
        Id = id;
        this.Order = order;
        Random random = new Random();
        if (random.Next() % 2 == 0)
        {
            this.Status = PaymentStatus.Approved;
        }
        else
        {
            this.Status = PaymentStatus.Rejected;
        }
    }
}
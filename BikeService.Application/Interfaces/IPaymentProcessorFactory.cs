namespace BikeService.Application.Interfaces
{
    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor? GetProcessor(string slug);
    }
}

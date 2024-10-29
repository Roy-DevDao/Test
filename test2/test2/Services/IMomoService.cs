using test2.Models.Momo;
using test2.Models.Order;
using Microsoft.AspNetCore.Http; // Needed for IQueryCollection

namespace test2.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}

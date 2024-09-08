using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

public class DiscountService (DiscountContext dbContext, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbContext.Coupons
            .FirstOrDefaultAsync(t => t.ProductName == request.ProductName);

        if (coupon is null)
            coupon = new() { ProductName = "No Discount", Description = "No Discount for product", Amount = 0 };

        logger.LogInformation($"Discount is retrieved for ProductName: {request.ProductName}.");

        return coupon.Adapt<CouponModel>();
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.Adapt<Coupon>();

        if (coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object."));

        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Discount is successfully created. ProductName: {request.Coupon.ProductName}, Amount: {request.Coupon.Amount}%.");

        return coupon.Adapt<CouponModel>();
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.Adapt<Coupon>();

        if (coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object."));

        if(!dbContext.Coupons.Any(t => t.ProductName == coupon.ProductName))
        {
            coupon = new() { ProductName = "No Discount", Description = "No Discount for product", Amount = 0 };
            return coupon.Adapt<CouponModel>();
        }


        //var coupon = await dbContext.Coupons
        //    .FirstOrDefaultAsync(t => t.ProductName == request.Coupon.ProductName);

        //if (coupon is null)
        //    coupon = new() { ProductName = "No Discount", Description = "No Discount for product", Amount = 0 };
        //else
        //{
        //    coupon.ProductName = request.Coupon.ProductName;
        //    coupon.Description = request.Coupon.Description;
        //    coupon.Amount = request.Coupon.Amount;
        //}

        dbContext.Coupons.Update(coupon);
        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Discount is successfully updated for ProductName: {request.Coupon.ProductName}.");

        return coupon.Adapt<CouponModel>();

    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var response = new DeleteDiscountResponse() { Success = false };
        var coupon = await dbContext.Coupons
            .FirstOrDefaultAsync(t => t.ProductName == request.ProductName);

        if (coupon is null)
            return response;

        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Discount is successfully removed for ProductName: {request.ProductName}.");

        response.Success = true;
        return response;
    }
}

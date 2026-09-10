using CloudShopping.Application.Abstractions.Files;
using CloudShopping.Application.Features.Products.Commands.UploadProductImage;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class UploadImageValidationTests
{
    [Theory]
    [InlineData(1,1,0,true)]
    [InlineData(0,1,0,false)]
    [InlineData(1,0,0,false)]
    [InlineData(1,10000001,0,false)]
    [InlineData(1,1,-1,false)]
    public void Upload_requires_identity_nonempty_bounded_file_and_valid_order(int product,long size,int order,bool expected)
    {
        using var stream=new MemoryStream([1]);
        var result=new UploadProductImageCommandValidator().Validate(new UploadProductImageCommand(product,new UploadFile("photo.jpg",size,stream),false,order));
        Assert.Equal(expected,result.IsValid);Assert.True(stream.CanRead);
    }
    [Fact]
    public void Closed_stream_is_rejected()
    {
        var stream=new MemoryStream([1]);stream.Dispose();
        Assert.False(new UploadProductImageCommandValidator().Validate(new UploadProductImageCommand(1,new("photo.jpg",1,stream),false,0)).IsValid);
    }
    [Fact]
    public void Missing_file_is_rejected_without_null_reference()
    {Assert.False(new UploadProductImageCommandValidator().Validate(new UploadProductImageCommand(1,null!,false,0)).IsValid);}
}

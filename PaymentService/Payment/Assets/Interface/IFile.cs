namespace Payment.Asset.Interface;

public interface IFile
{
    [GraphQLType(typeof(NonNullType<UploadType>))]
    public IFile Upload { get; set; }
}
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

public class ApplicationProject : ApplicationProject<string>
{
    public ApplicationProject()
    {
        ID = System.Guid.NewGuid().ToString();
    }
}

public class ApplicationProject<TKey> where TKey : IEquatable<TKey>
{
    [PersonalData]
    public virtual TKey ID { get; set; } = default!;
    [ProtectedPersonalData]
    public virtual List<string> Contributors { get; set; } = new List<string>();
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Description { get; set; } = string.Empty;
    public virtual string Image { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

public class ProjectContributor
{
    public string ProjectId { get; set; }
    public ApplicationProject Project { get; set; }

    public string Contributor { get; set; }
}

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
    public virtual ICollection<ProjectContributor> Contributors { get; set; } = new List<ProjectContributor>();
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Description { get; set; } = string.Empty;
    public virtual string Image { get; set; } = string.Empty;
}


using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;
public class RealTreeNodeTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;

    public RealTreeNodeTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
    }

    [Fact]
    public void CreateContainer_WithoutParameters_GeneratesIdAndName()
    {
        var container = _factory.CreateContainer();

        Assert.NotEqual(Guid.Empty, container.Id);
        Assert.NotNull(container.Name);
        Assert.NotEmpty(container.Name);
    }

    [Fact]
    public void CreateContainer_WithId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var container = _factory.CreateContainer(id, "Test");

        Assert.Equal(id, container.Id);
    }

    [Fact]
    public void CreateContainer_WithName_UsesProvidedName()
    {
        var container = _factory.CreateContainer(name: "CustomName");

        Assert.Equal("CustomName", container.Name);
    }

    [Fact]
    public void CreateItem_WithoutParameters_GeneratesIdAndName()
    {
        var item = _factory.CreateItem();

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.NotNull(item.Name);
        Assert.NotEmpty(item.Name);
    }

    [Fact]
    public void CreateTree_WithoutParameters_CreatesRootWithDefaults()
    {
        var tree = _factory.CreateTree();

        Assert.NotEqual(Guid.Empty, tree.Id);
        Assert.NotNull(tree.Name);
    }

    [Fact]
    public void CreateTree_WithName_UsesProvidedName()
    {
        var tree = _factory.CreateTree(name: "MyTree");

        Assert.Equal("MyTree", tree.Name);
    }

    [Fact]
    public void Name_CanBeChanged()
    {
        var container = _factory.CreateContainer(name: "OldName");

        container.Name = "NewName";

        Assert.Equal("NewName", container.Name);
    }

    [Fact]
    public void Name_ThrowsWhenSetToNull()
    {
        var container = _factory.CreateContainer(name: "Test");

        Assert.Throws<ArgumentException>(() => container.Name = null);
    }

    [Fact]
    public void Name_ThrowsWhenSetToWhitespace()
    {
        var container = _factory.CreateContainer(name: "Test");

        Assert.Throws<ArgumentException>(() => container.Name = "   ");
    }

    [Fact]
    public void Metadata_CanStoreValues()
    {
        var container = _factory.CreateContainer();

        container.Metadata["key1"] = "value1";
        container.Metadata["key2"] = 42;
        container.Metadata["key3"] = true;

        Assert.Equal("value1", container.Metadata["key1"]);
        Assert.Equal(42, container.Metadata["key2"]);
        Assert.Equal(true, container.Metadata["key3"]);
    }

    [Fact]
    public void Metadata_IsEmptyByDefault()
    {
        var container = _factory.CreateContainer();

        Assert.Empty(container.Metadata);
    }

    [Fact]
    public void Tree_ThrowsWhenNodeNotAttached()
    {
        var container = _factory.CreateContainer();

        Assert.Throws<InvalidOperationException>(() => container.Tree);
    }

    [Fact]
    public void Dispose_DisposesTree()
    {
        var tree = _factory.CreateTree();

        tree.Dispose();

        // Subsequent dispose should not throw
        tree.Dispose();
    }
}
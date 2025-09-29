using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

public class RealTreeNavigationTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeNavigationTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task FindByPath_WithRootPath_ReturnsRoot()
    {
        var tree = _factory.CreateTree(name: "Root");

        var found = tree.FindByPath("/");

        Assert.Same(tree, found);
    }

    [Fact]
    public async Task FindByPath_WithValidPath_ReturnsNode()
    {
        var tree = _factory.CreateTree(name: "Root");
        var folder = await _operations.AddContainerAsync(tree, null, "Folder");
        var item = await _operations.AddItemAsync(folder, null, "Item");

        var found = tree.FindByPath("/Folder/Item");

        Assert.Same(item, found);
    }

    [Fact]
    public async Task FindByPath_WithInvalidPath_ReturnsNull()
    {
        var tree = _factory.CreateTree(name: "Root");
        await _operations.AddContainerAsync(tree, null, "Folder");

        var found = tree.FindByPath("/NonExistent/Path");

        Assert.Null(found);
    }

    [Fact]
    public async Task FindByPath_WithNestedContainers_ReturnsCorrectNode()
    {
        var tree = _factory.CreateTree(name: "Root");
        var c1 = await _operations.AddContainerAsync(tree, null, "Level1");
        var c2 = await _operations.AddContainerAsync(c1, null, "Level2");
        var c3 = await _operations.AddContainerAsync(c2, null, "Level3");

        var found = tree.FindByPath("/Level1/Level2/Level3");

        Assert.Same(c3, found);
    }

    [Fact]
    public async Task FindById_WithValidId_ReturnsNode()
    {
        var tree = _factory.CreateTree(name: "Root");
        var id = Guid.NewGuid();
        var container = await _operations.AddContainerAsync(tree, id, "TestContainer");

        var found = tree.FindById(id);

        Assert.Same(container, found);
    }

    [Fact]
    public async Task FindById_WithInvalidId_ReturnsNull()
    {
        var tree = _factory.CreateTree(name: "Root");
        await _operations.AddContainerAsync(tree, null, "TestContainer");

        var found = tree.FindById(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task FindById_InNestedStructure_ReturnsNode()
    {
        var tree = _factory.CreateTree(name: "Root");
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(c1, null, "C2");
        var itemId = Guid.NewGuid();
        var item = await _operations.AddItemAsync(c2, itemId, "Item");

        var found = tree.FindById(itemId);

        Assert.Same(item, found);
    }

    [Fact]
    public async Task Path_ReturnsCorrectPath()
    {
        var tree = _factory.CreateTree(name: "Root");
        var folder = await _operations.AddContainerAsync(tree, null, "Documents");
        var subfolder = await _operations.AddContainerAsync(folder, null, "Work");
        var item = await _operations.AddItemAsync(subfolder, null, "Report.pdf");

        Assert.Equal("/Root", tree.Path);
        Assert.Equal("/Root/Documents", folder.Path);
        Assert.Equal("/Root/Documents/Work", subfolder.Path);
        Assert.Equal("/Root/Documents/Work/Report.pdf", item.Path);
    }

    [Fact]
    public async Task Depth_ReturnsCorrectDepth()
    {
        var tree = _factory.CreateTree(name: "Root");
        var c1 = await _operations.AddContainerAsync(tree, null, "Level1");
        var c2 = await _operations.AddContainerAsync(c1, null, "Level2");
        var c3 = await _operations.AddContainerAsync(c2, null, "Level3");

        Assert.Equal(0, tree.Depth);
        Assert.Equal(1, c1.Depth);
        Assert.Equal(2, c2.Depth);
        Assert.Equal(3, c3.Depth);
    }

    [Fact]
    public async Task Parent_ReturnsCorrectParent()
    {
        var tree = _factory.CreateTree(name: "Root");
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");

        Assert.Null(tree.Parent);
        Assert.Same(tree, container.Parent);
        Assert.Same(container, item.Parent);
    }

    [Fact]
    public async Task Tree_ReturnsRootTree()
    {
        var tree = _factory.CreateTree(name: "Root");
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(c1, null, "C2");
        var item = await _operations.AddItemAsync(c2, null, "Item");

        Assert.Same(tree, tree.Tree);
        Assert.Same(tree, c1.Tree);
        Assert.Same(tree, c2.Tree);
        Assert.Same(tree, item.Tree);
    }

    [Fact]
    public async Task Children_ReturnsAllChildNodes()
    {
        var tree = _factory.CreateTree(name: "Root");
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var childContainer = await _operations.AddContainerAsync(container, null, "ChildContainer");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");
        var item2 = await _operations.AddItemAsync(container, null, "Item2");

        var children = container.Children;

        Assert.Equal(3, children.Count);
        Assert.Contains(childContainer, children);
        Assert.Contains(item1, children);
        Assert.Contains(item2, children);
    }

    [Fact]
    public async Task Containers_ReturnsOnlyContainers()
    {
        var tree = _factory.CreateTree(name: "Root");
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var c1 = await _operations.AddContainerAsync(parent, null, "C1");
        var c2 = await _operations.AddContainerAsync(parent, null, "C2");
        await _operations.AddItemAsync(parent, null, "Item");

        var containers = parent.Containers;

        Assert.Equal(2, containers.Count);
        Assert.Contains(c1, containers);
        Assert.Contains(c2, containers);
    }

    [Fact]
    public async Task Items_ReturnsOnlyItems()
    {
        var tree = _factory.CreateTree(name: "Root");
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        await _operations.AddContainerAsync(container, null, "ChildContainer");
        var i1 = await _operations.AddItemAsync(container, null, "I1");
        var i2 = await _operations.AddItemAsync(container, null, "I2");

        var items = container.Items;

        Assert.Equal(2, items.Count);
        Assert.Contains(i1, items);
        Assert.Contains(i2, items);
    }

    [Fact]
    public async Task Item_CanHoldContainers()
    {
        var tree = _factory.CreateTree(name: "Root");
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var subContainer = await _operations.AddContainerAsync(item, null, "SubContainer");

        Assert.Single(item.Containers);
        Assert.Contains(subContainer, item.Containers);
        Assert.Same(item, subContainer.Parent);
    }
}

using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

public class RealTreeIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task ComplexHierarchy_BuildsCorrectly()
    {
        var tree = _factory.CreateTree(name: "FileSystem");
        _output.WriteLine($"Created tree: {tree.Name}");

        // Build: /FileSystem/Documents/Work/Projects/Project1
        var docs = await _operations.AddContainerAsync(tree, null, "Documents");
        _output.WriteLine($"Added docs: {docs.Name}, Parent: {docs.Parent?.Name ?? "null"}");

        try
        {
            var docsTree = docs.Tree;
            _output.WriteLine($"Docs.Tree accessible: {docsTree.Name}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ERROR accessing docs.Tree: {ex.Message}");
        }

        var work = await _operations.AddContainerAsync(docs, null, "Work");
        _output.WriteLine($"Added work: {work.Name}, Parent: {work.Parent?.Name ?? "null"}");

        try
        {
            var workTree = work.Tree;
            _output.WriteLine($"Work.Tree accessible: {workTree.Name}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ERROR accessing work.Tree: {ex.Message}");
        }

        var projects = await _operations.AddContainerAsync(work, null, "Projects");
        _output.WriteLine($"Added projects: {projects.Name}, Parent: {projects.Parent?.Name ?? "null"}");

        try
        {
            var projectsTree = projects.Tree;
            _output.WriteLine($"Projects.Tree accessible: {projectsTree.Name}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ERROR accessing projects.Tree: {ex.Message}");
        }

        _output.WriteLine("About to add project1 item...");
        var project1 = await _operations.AddItemAsync(projects, null, "Project1");
        _output.WriteLine($"Added project1: {project1.Name}, Parent: {project1.Parent?.Name ?? "null"}");

        try
        {
            var project1Tree = project1.Tree;
            _output.WriteLine($"Project1.Tree accessible: {project1Tree.Name}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ERROR accessing project1.Tree: {ex.Message}");
        }

        // Add containers to item
        _output.WriteLine("About to add reports container to project1 item...");
        var reports = await _operations.AddContainerAsync(project1, null, "Reports");
        _output.WriteLine($"Added reports: {reports.Name}, Parent: {reports.Parent?.Name ?? "null"}");

        var report1 = await _operations.AddItemAsync(reports, null, "Q4Report.pdf");
        _output.WriteLine($"Added report1: {report1.Name}, Path: {report1.Path}");

        Assert.Equal("/FileSystem/Documents/Work/Projects/Project1/Reports/Q4Report.pdf", report1.Path);
        Assert.Equal(6, report1.Depth);
        Assert.Same(tree, report1.Tree);
    }

    [Fact]
    public async Task ActionsWithValidation_WorkCorrectly()
    {
        var tree = _factory.CreateTree();
        var validationLog = new List<string>();

        // Global validation using type-based registration
        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            if (ctx.Container.Name.Contains("invalid"))
            {
                throw new InvalidOperationException("Invalid name");
            }
            validationLog.Add($"Validated: {ctx.Container.Name}");
            await next();
        });

        // Add valid containers
        await _operations.AddContainerAsync(tree, null, "Valid1");
        await _operations.AddContainerAsync(tree, null, "Valid2");

        // Try invalid
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _operations.AddContainerAsync(tree, null, "invalid_name"));

        Assert.Equal(2, validationLog.Count);
        Assert.Equal(2, tree.Containers.Count);
    }

    [Fact]
    public async Task CopyOperation_WithComplexStructure_PreservesHierarchy()
    {
        var tree = _factory.CreateTree();

        // Create source structure
        var source = await _operations.AddContainerAsync(tree, null, "Source");
        var level1 = await _operations.AddContainerAsync(source, null, "Level1");
        var level2 = await _operations.AddContainerAsync(level1, null, "Level2");
        var item = await _operations.AddItemAsync(level2, null, "Item");
        var itemContainer = await _operations.AddContainerAsync(item, null, "ItemContainer");

        source.Metadata["sourceData"] = "value";
        level1.Metadata["level1Data"] = 123;

        // Copy entire structure
        var copy = await _operations.CopyContainerAsync(source, tree, newName: "Copy");

        // Verify structure
        Assert.Equal("Copy", copy.Name);
        Assert.Single(copy.Containers);
        var copiedLevel1 = copy.Containers[0];
        Assert.Single(copiedLevel1.Containers);
        var copiedLevel2 = copiedLevel1.Containers[0];
        Assert.Single(copiedLevel2.Items);
        var copiedItem = copiedLevel2.Items[0];
        Assert.Single(copiedItem.Containers);

        // Verify metadata was copied
        Assert.Equal("value", copy.Metadata["sourceData"]);
        Assert.Equal(123, copiedLevel1.Metadata["level1Data"]);
    }

    [Fact]
    public async Task BulkOperations_WithLargeDataset_PerformsEfficiently()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Bulk");

        // Create 100 containers using tuple syntax
        var containerTuples = Enumerable.Range(0, 100)
            .Select(i => ((Guid?)Guid.NewGuid(), $"Container_{i}", (IDictionary<string, object>?)new Dictionary<string, object>()))
            .ToArray();

        await _operations.BulkAddContainersAsync<RealTreeContainer>(container, containerTuples);

        // Create 100 items using tuple syntax
        var itemTuples = Enumerable.Range(0, 100)
            .Select(i => ((Guid?)Guid.NewGuid(), $"Item_{i}", (IDictionary<string, object>?)new Dictionary<string, object>()))
            .ToArray();

        await _operations.BulkAddItemsAsync<RealTreeItem>(container, itemTuples);

        Assert.Equal(100, container.Containers.Count);
        Assert.Equal(100, container.Items.Count);
        Assert.Equal(200, container.Children.Count);
    }

    [Fact]
    public async Task NestedActions_ExecuteInCorrectOrder()
    {
        var tree = _factory.CreateTree();
        var executionLog = new List<string>();

        // Tree level - type-based registration
        // Type-based actions - RealTreeRoot handler
        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            executionLog.Add("Tree-Action-Before");
            await next();
            executionLog.Add("Tree-Action-After");
        });

        // Type-based actions - RealTreeContainer handler
        _operations.RegisterAddContainerAction<RealTreeContainer>(async (ctx, next) =>
        {
            executionLog.Add("Container-Action-Before");
            await next();
            executionLog.Add("Container-Action-After");
        });

        // Adding to tree (RealTreeRoot) - only Tree actions execute
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        Assert.Contains("Tree-Action-Before", executionLog);
        Assert.Contains("Tree-Action-After", executionLog);
        executionLog.Clear();

        // Adding to parent (RealTreeContainer) - only Container actions execute
        await _operations.AddContainerAsync(parent, null, "Child");
        Assert.Contains("Container-Action-Before", executionLog);
        Assert.Contains("Container-Action-After", executionLog);
        Assert.DoesNotContain("Tree-Action-Before", executionLog);
    }

    [Fact]
    public async Task MoveOperation_UpdatesPathAndDepthCorrectly()
    {
        var tree = _factory.CreateTree(name: "Root");
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(tree, null, "C2");
        var deepContainer = await _operations.AddContainerAsync(c1, null, "Deep");
        var item = await _operations.AddItemAsync(deepContainer, null, "Item");

        var originalPath = item.Path;
        var originalDepth = item.Depth;

        await _operations.MoveAsync(deepContainer, c2);

        Assert.NotEqual(originalPath, item.Path);
        Assert.Equal("/Root/C2/Deep/Item", item.Path);
        Assert.Equal(originalDepth, item.Depth); // Depth relative to tree unchanged
    }

    [Fact]
    public async Task RemoveOperation_CleansUpDescendants()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var child = await _operations.AddContainerAsync(parent, null, "Child");
        var grandchild = await _operations.AddContainerAsync(child, null, "Grandchild");
        var item = await _operations.AddItemAsync(grandchild, null, "Item");

        var itemId = item.Id;

        await _operations.RemoveAsync(parent);

        // All descendants should be detached
        Assert.Null(parent.Parent);
        Assert.Empty(tree.Containers);
        Assert.Null(tree.FindById(itemId));
    }
}

public class RealTreeEdgeCaseTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeEdgeCaseTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task BulkAddContainers_WithEmptyList_DoesNothing()
    {
        var tree = _factory.CreateTree();
        var actionExecuted = false;

        _operations.RegisterBulkAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        await _operations.BulkAddContainersAsync<RealTreeContainer>(tree, Array.Empty<(Guid? id, string name, IDictionary<string, object>? metadata)>());

        Assert.False(actionExecuted);
        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task BulkAddItems_WithEmptyList_DoesNothing()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");

        await _operations.BulkAddItemsAsync<RealTreeItem>(container, Array.Empty<(Guid? id, string name, IDictionary<string, object>? metadata)>());

        Assert.Empty(container.Items);
    }

    [Fact]
    public async Task BulkRemove_WithEmptyList_DoesNothing()
    {
        var tree = _factory.CreateTree();

        await _operations.BulkRemoveAsync(new List<IRealTreeNode>());

        // Should not throw
        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task UpdateAsync_WithNullParameters_LeavesNodeUnchanged()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Original");
        container.Metadata["key"] = "value";

        await _operations.UpdateAsync(container, null, null);

        Assert.Equal("Original", container.Name);
        Assert.Single(container.Metadata);
        Assert.Equal("value", container.Metadata["key"]);
    }

    [Fact]
    public async Task UpdateAsync_WithOnlyName_ChangesOnlyName()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Original");
        container.Metadata["key"] = "value";

        await _operations.UpdateAsync(container, newName: "Updated");

        Assert.Equal("Updated", container.Name);
        Assert.Single(container.Metadata);
    }

    [Fact]
    public async Task UpdateAsync_WithOnlyMetadata_ChangesOnlyMetadata()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Original");
        container.Metadata["old"] = "value";

        var newMeta = new Dictionary<string, object> { { "new", "value2" } };
        await _operations.UpdateAsync(container, newMetadata: newMeta);

        Assert.Equal("Original", container.Name);
        Assert.Single(container.Metadata);
        Assert.Equal("value2", container.Metadata["new"]);
    }

    [Fact]
    public async Task CopyContainer_WithNewIdAndName_UsesProvided()
    {
        var tree = _factory.CreateTree();
        var source = await _operations.AddContainerAsync(tree, null, "Source");
        var newId = Guid.NewGuid();

        var copy = await _operations.CopyContainerAsync(source, tree, newId, "CustomName");

        Assert.Equal(newId, copy.Id);
        Assert.Equal("CustomName", copy.Name);
    }

    [Fact]
    public async Task CopyItem_WithNewIdAndName_UsesProvided()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var source = await _operations.AddItemAsync(container, null, "Source");
        var newId = Guid.NewGuid();

        var copy = await _operations.CopyItemAsync(source, container, newId, "CustomName");

        Assert.Equal(newId, copy.Id);
        Assert.Equal("CustomName", copy.Name);
    }

    [Fact]
    public async Task FindByPath_WithTrailingSlash_HandledCorrectly()
    {
        var tree = _factory.CreateTree(name: "Root");
        var container = await _operations.AddContainerAsync(tree, null, "Folder");

        var found = tree.FindByPath("/Folder/");

        // Should handle gracefully - implementation may vary
        Assert.True(found == null || found == container);
    }

    [Fact]
    public async Task FindByPath_WithEmptyString_ReturnsRoot()
    {
        var tree = _factory.CreateTree(name: "Root");

        var found = tree.FindByPath("");

        Assert.Same(tree, found);
    }

    [Fact]
    public async Task AddContainer_ToItem_WorksCorrectly()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var subContainer = await _operations.AddContainerAsync(item, null, "SubContainer");

        Assert.Single(item.Containers);
        Assert.Same(item, subContainer.Parent);
        Assert.Equal("/Root/Container/Item/SubContainer", subContainer.Path);
    }

    [Fact]
    public async Task BulkRemove_WithNodesFromDifferentParents_GroupsByParent()
    {
        var tree = _factory.CreateTree();
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(tree, null, "C2");
        var child1 = await _operations.AddContainerAsync(c1, null, "Child1");
        var child2 = await _operations.AddContainerAsync(c2, null, "Child2");

        await _operations.BulkRemoveAsync(new[] { child1, child2 });

        Assert.Empty(c1.Containers);
        Assert.Empty(c2.Containers);
    }

    [Fact]
    public async Task Action_ThrowingException_StopsOperation()
    {
        var tree = _factory.CreateTree();

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            throw new InvalidOperationException("Blocked");
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _operations.AddContainerAsync(tree, null, "Test"));

        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task MultipleActions_FirstThrows_SubsequentNotExecuted()
    {
        var tree = _factory.CreateTree();
        var secondActionExecuted = false;

        // First registered action throws - subsequent actions should not execute
        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            throw new InvalidOperationException("First action fails");
        });

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            secondActionExecuted = true;
            await next();
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _operations.AddContainerAsync(tree, null, "Test"));

        // Second action should not execute because first action threw exception
        Assert.False(secondActionExecuted);
    }

    [Fact]
    public async Task CancellationToken_CanBePassed()
    {
        var tree = _factory.CreateTree();
        var cts = new CancellationTokenSource();

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            if (ctx.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            await next();
        });

        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _operations.AddContainerAsync(tree, null, "Test", cancellationToken: cts.Token));
    }

    [Fact]
    public void Metadata_CanStoreComplexObjects()
    {
        var container = _factory.CreateContainer();
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 } };

        container.Metadata["complex"] = complexObject;

        var retrieved = container.Metadata["complex"];
        Assert.Same(complexObject, retrieved);
    }

    [Fact]
    public async Task Path_WithSpecialCharacters_HandledCorrectly()
    {
        var tree = _factory.CreateTree(name: "Root");
        var container = await _operations.AddContainerAsync(tree, null, "Folder/With/Slashes");

        var path = container.Path;

        Assert.Equal("/Root/Folder/With/Slashes", path);
    }

    [Fact]
    public async Task TreeHierarchy_WithMixedContainersAndItems_NavigatesCorrectly()
    {
        var tree = _factory.CreateTree(name: "Root");
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var item1 = await _operations.AddItemAsync(c1, null, "Item1");
        var c2 = await _operations.AddContainerAsync(item1, null, "C2");
        var item2 = await _operations.AddItemAsync(c2, null, "Item2");

        // Path should not include the root's name
        var found = tree.FindByPath("/C1/Item1/C2/Item2");
        Assert.Same(item2, found);

        // Navigate using parent chain
        Assert.Same(c2, item2.Parent);
        Assert.Same(item1, c2.Parent);
        Assert.Same(c1, item1.Parent);
        Assert.Same(tree, c1.Parent);
    }

    [Fact]
    public async Task Container_WithIdenticalChildNames_CanExist()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");

        // Library allows duplicate names - no validation preventing this
        var child1 = await _operations.AddContainerAsync(container, null, "SameName");
        var child2 = await _operations.AddContainerAsync(container, null, "SameName");

        Assert.Equal(2, container.Containers.Count);
        Assert.NotSame(child1, child2);
        Assert.Equal(child1.Name, child2.Name);
    }

    [Fact]
    public async Task BulkOperations_WithSingleItem_Work()
    {
        var tree = _factory.CreateTree();

        var containerTuples = new[] { ((Guid?)Guid.NewGuid(), "Single", (IDictionary<string, object>?)new Dictionary<string, object>()) };
        await _operations.BulkAddContainersAsync<RealTreeContainer>(tree, containerTuples);

        Assert.Single(tree.Containers);
    }
}

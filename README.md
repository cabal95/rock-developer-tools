# Rock Developer Tools

This is a set of blocks and (eventually) other tools to help developers quickly produce plugins.

One of the things we all fight is taking a functional chunk of code... A block, job, maybe just
a page tree we have built... and turning it into something useful that can be distributed to
others. That requires re-producing what we have built in our dev environment into something
that can be uploaded to the Rock shop.

There are some SQL scripts included in the Rock repo to help with some of this - but let's face it.
SQL was never designed to generate C# code, so it can be a bit clunky to work with. And if you
need to make modifications to the styled output that can be even more difficult. But these were
the quickest way to get a head start back when they were created.

With that in mind, I started creating a small collection of blocks and Lava templates to help
do just that. We are familiar with Lava and it's a good deal cleaner to stylize the way we
want than SQL is. Additionally, because there is now a UI and some C# code powering it we
can simplify the interface even more.

## Blocks

Currently there are three blocks for generating code:

1. Entity Types
2. Block Types
3. Page Tree
4. Block Template (probably needs a better name)

There is also a set of Lava files that generate the output nearly identical to the current SQL
CodeGen scripts (with the exception of the page tree as I think currently the SQL script only
exports a single page at a time).

### Sample Output

I'm not going to include actual sample output as that would waste a ton of space. But I'll give
you a brief overview of what the default lava for each block provides.

#### Entity Types

1. The `Up()` method that contains the `UpdateEntityType()` method to initialize each entity type.
2. The `Down()` method (with a warning about probably not using it in practice).
3. The `SystemGuid.EntityType` class with all the GUIDs for each of those entity types.

#### Block Types

1. The `Up()` method that contains the `AddBlockType()` and any `AddBlockTypeAttribute()` method calls
to register each block type.
2. The `Down()` method to delete those block types.
3. The `SystemGuid.BlockType` class with all the GUIDs for each of those block types.
4. The `SystemGuid.Attribute` class with all the GUIDs for each of those block types.

#### Page Tree

1. The `Up()` method that contains the `AddPage()`, `AddBlock()` and `AddBlockAttributeValue()` method
calls to create each page and add the blocks.
2. The `Down()` method with the `DeleteBlock()` and `DeletePage()` calls.
3. A `SystemGuid.BlockType` class that contains GUID values for any core block types.
4. The `SystemGuid.Page` class that contains the GUID values for the pages.
5. The `SystemGuid.Block` class that contains teh GUID values for the blocks.

### Block Templates

This block doesn't match the previous blocks in terms of output. It's design is to have you pick
an Entity Type from the database and it then will generate a standard "List" and "Detail" block
`.ascx` and `.ascx.cs` files.

#### List Block

When generating a list block, it will give you a selection of checkboxes so that you can pick
which properties appear on the grid columns and which properties appear in the grid filter section.

There is some fine tuning that could still be done, such as automating checks if the entity service
has a `CanDelete` method before generating one. But even as it is now, it should get you started
and save you a fair amount of time.

#### Detail Block (not fully written yet)

Similarly, this mode will let you pick which properties are displayed and can be edited by
the user (for example, you probably don't want to let them edit the `IsSystem` property). You can
also indicate if this is an "edit only" block (like the File Type Detail block) or if it should
support a View and then Edit mode (like the Financial Account Detail block).

### Customizing Output

There is a Custom folder that contains some samples of ways I customized the standard output for
my own uses. In my more complex projects, I've started building class hierarchies for the page
and block GUIDs because I found that I had pages and blocks with the same names. It was getting a
bit unwieldly so I adopted the form of `SystemGuid.Page.WatchdogMonitor.DeviceList.DeviceDetails.GUID`
to denote the `Device Details` page that is a child of the `Device List` page that is a child of
the `Watchdog Monitor` page.

Another thing that could (and probably should) be added to this, is the SQL required to undo a
migration. Since the `Down()` methods are never called (though another developer has created a block
that allows you to run the `Down()` methods), it's hard to undo a migration when you find a bug in
it while developing. Adding a section to the Lava that generates the SQL required to undo all
tasks performed in the `Up()` method would be trivial.

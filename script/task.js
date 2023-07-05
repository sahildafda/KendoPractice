$(document).ready(function () {
    $("#splitter").kendoSplitter({
        orientation: "horizontal",
        panes: [
            { collapsible: true, size: "300px" },
            { collapsible: false }
        ]
    });


    // Data for the TreeView
    $.ajax({
        url: "http://localhost:35876/api/User/GetDatabases",
        method: "GET",
        dataType: "json",
        success: function (response) {
            var hierarchicalData = [];

            response.forEach(function (database) {
                var databaseNode = {
                    text: database.databaseName,
                    spriteCssClass: "fa fa-database",
                    items: []
                };

                database.allTables.forEach(function (table) {
                    var tableNode = {
                        text: table.schemaName + "." + table.tableName,
                        spriteCssClass: "fa fa-table"
                    };

                    databaseNode.items.push(tableNode);
                });

                hierarchicalData.push(databaseNode);
            });

            $("#treeview").kendoTreeView({
                dataSource: hierarchicalData,
                select: onselect
            });

            // // Handle click event on table nodes
            // $("#treeview").on("click", ".k-item", function () {
            //     var selectedText = $(this).text();
            //     console.log("Clicked element text:", selectedText);
            // });
        },
        error: function (xhr, status, error) {
            console.log("Error retrieving API data:", error);
        }
    });


});


async function onselect(e) {
    var current = this.text(e.node);// `this` refers to the treeview object

    var parent = this.parent(e.node);
    var parentText = this.text(parent);

    var tableDetails = parentText + "." + current;


    var grid = $("#grid").data("kendoGrid");


    // check grid is empty or not
    var grid = $("#grid").data("kendoGrid")

    if (grid == undefined) {
        console.log("grid is empty");
    }
    else {
        console.log("full");
        $('#grid').kendoGrid('destroy').empty();
    }

    console.log(tableDetails);


    $("#grid").kendoGrid({

        dataSource: {
            transport: {
                read: {
                    url: "http://localhost:35876/api/User/GetFullTableData/" + tableDetails,
                    dataType: "json"
                }
            },
            pageSize: 10 // Adjust the page size as per your needs
        },

        height: 450,
        sortable: true,
        groupable: true, // Adjust the grid height as per your needs
        pageable: true,
        filterable: true
    });
}

function checkGridData() {

}
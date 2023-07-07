var baseApi = "http://localhost:35876/api/User/";
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
        url: baseApi + "GetDatabases",
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

// http://localhost:35876/api/User/GetFullTableData/" + tableDetails

function onselect(e) {
    var current = this.text(e.node);// `this` refers to the treeview object

    var parent = this.parent(e.node);
    var parentText = this.text(parent);
    var columns = [];
    var tableDetails = parentText + "." + current;
    var columnList = [];


    var grid = $("#grid").data("kendoGrid");





    $.ajax({
        url: baseApi + "getcols/" + tableDetails,
        success: function (response) {
            var columnList = response;


            columns = columnList.map(function (columnName) {
                return { columnName };
            });

            // columns.push({ command: ["edit", "destroy"], title: "&nbsp;", width: "250px" });
            console.log(columns);
        },
        error: function (err) {
            console.log("error : " + err);
        }
    });


    if (grid == undefined) {
        console.log("grid is empty");
    }
    else {
        console.log("full");
        $('#grid').kendoGrid('destroy').empty();
    }

    console.log(tableDetails);

    var oldData = [];
    var newData = [];
    var tableName = tableDetails;

    var dataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: baseApi + 'GetFullTableData/' + tableDetails,
                dataType: "json"
            },
        },
        requestEnd: function (e) {
            if (e.type == "read") {
                // Refresh the grid to reflect the changes
                console.log("getting data");


            }
            else {
                dataSource.read();

            }
        },
        pageSize: 10 // Adjust the page size as per your needs
    })




    $("#grid").kendoGrid({

        dataSource: dataSource,
        height: 450,
        sortable: true,
        groupable: true, // Adjust the grid height as per your needs
        pageable: true,
        filterable: true,
        toolbar: ["create"], // Add toolbar buttons    
        editable: {
            mode: "popup", // Set the edit mode as "popup"
            window: {
                title: "Edit", // Customize the edit popup window title as needed
                animation: false,
            }
        },
        dataBound: function () {
            var grid = this;
            var rows = grid.tbody.find("tr");

            rows.each(function (index, row) {
                var rowData = grid.dataItem(row);
                var editButtonHtml = '<td><a class="k-button k-button-icontext k-grid-edit" href="\\#"><span class="k-icon k-i-edit"></span>Edit</a><a class="k-button k-button-icontext k-grid-delete" href="\\#"><span class="k-icon k-i-edit"></span>delete</a></td>';

                $(row).append(editButtonHtml);
            });
        },
        save: function (e) {
            console.log("saved");



            // console.log(updateData);

            if (Object.keys(oldData).length === 0) {


                // var firstInput = $(".k-edit-field:first-child");
                // firstInput.prop("readonly", true);
                // firstInput.val("1");

                newData = e.model;


                var insertData = {};

                insertData.tableName = tableName;
                insertData.updated_data = newData;

                console.log(insertData);


                console.log("calling insert query");
                $.ajax({
                    url: baseApi + "TableInserter",
                    method: "POST", // Assuming you want to send a POST request
                    data: JSON.stringify(insertData), // Convert removeData to a JSON string
                    contentType: "application/json", // Set the request content type
                    success: function () {
                        console.log("Data Inserted");

                        // Assuming you have a button with an ID "myButton"
                        $(".k-grid-cancel").click();

                        dataSource.read();
                    },
                    error: function (xhr, status, error) {
                        console.log("Error:", error);
                    }
                });
            }
            else {

                newData = e.model;
                console.log("tblName : " + tableName);
                console.log(oldData);

                var updateData = {};

                updateData.tableName = tableName;
                updateData.old_data = oldData;

                updateData.updated_data = newData;

                console.log(updateData);


                console.log("calling update query");
                console.log(oldData.toString());
                $.ajax({
                    url: baseApi + "TableUpdater",
                    method: "POST", // Assuming you want to send a POST request
                    data: JSON.stringify(updateData), // Convert removeData to a JSON string
                    contentType: "application/json", // Set the request content type
                    success: function () {
                        console.log("Data Updated");

                        // Assuming you have a button with an ID "myButton"
                        $(".k-grid-cancel").click();

                        dataSource.read();


                    },
                    error: function (xhr, status, error) {
                        console.log("Error:", error);
                    }
                });
            }



        },
        remove: function (e) {
            var removeData = {};

            removeData.tableName = tableName;
            removeData.old_data = e.model;

            console.log(removeData);

            $.ajax({
                url: baseApi + "TableDeleter",
                method: "POST", // Assuming you want to send a POST request
                data: JSON.stringify(removeData), // Convert removeData to a JSON string
                contentType: "application/json", // Set the request content type
                success: function () {
                    console.log("Data Deleted");

                    dataSource.read();

                },
                error: function (xhr, status, error) {
                    console.log("Error:", error);
                }
            });


        },
        edit: function (e) {

            // Assuming you have an element with the class "k-numerictextbox" and want to hide its first child
            // $(".k-numerictextbox:first-child").hide();
            // var firstInput = $(".k-edit-field:first-child");
            // firstInput.prop("readonly", true);
            // firstInput.val("1");

            oldData = JSON.parse(JSON.stringify(e.model));

            console.log(oldData);

        },

    });
}

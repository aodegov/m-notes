
$(document).ready(function () {

    var summary_table = $('#sumTable').DataTable({

        "ajax": {
            "url": $('#queryGetSummary').data('request-url'),
            "dataSrc": ""
        },
        "dom": '<<t>lp>',
        "columns": [

            {
                "orderable": false,
                "data": null,
                "defaultContent": ''
            },

            {
                "orderable": false,
                "data": null,
                "defaultContent": ''
            },
            {
                "data": "Status",
                "render": function (data, type, full, meta) {
                    return '<a href="#">' + data + '</a>';
                }
            },
            { "data": "MRN" },
            { "data": "PersonName" },
            {
                "data": "DOB",
                "sType": "date"
            },
            { "data": "NoteType" },
            {
                "data": "NoteDate",
                "sType": "date",
            },
            { "data": "Provider" },
            {
                "data": "DxFound",
                "className": "right"
            },
            {
                "data": "DxAccepted",
                "className": "right"
            },
            {
                "data": "DxRejected",
                "className": "right"
            },
            {
                "data": "DxAdded",
                "className": "right"
            },
            {
                "data": "Note_id",
                "visible": false
            },
            {
                "data": "Client_Id",
                "visible": false
            }

        ],
        "fnCreatedRow": function (nRow, aData, iDataIndex) {

            if (aData["Status"] == "Complete") {
                $('td:eq(0)', nRow).css('background', 'linear-gradient(90deg, #5cb85c 30%, white 35%)');
            } else if (aData["Status"] == "In progress") {
                $('td:eq(0)', nRow).css('background', 'linear-gradient(90deg, #337ab7 30%, white 35%)');
            }
            else if (aData["Status"] == "Not processed") {
                $('td:eq(0)', nRow).css('background', 'linear-gradient(90deg, #c9302c 30%, white 35%)');
            }
            else if (aData["Status"] == "New found Dx") {
                $('td:eq(0)', nRow).css('background', 'linear-gradient(90deg, #31b0d5 30%, white 35%)');
            }

            if (aData["InnerData"].trim() != '') {
                $('td:eq(1)', nRow).addClass('details-control');
                Set_Nested_Icon(nRow, aData["Status"], true);
            }
        },
        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            //console.log(last_id);
            if (aData["Note_id"] == localStorage.getItem("last_id") && !($(nRow).hasClass('selected'))) {
                summary_table.$('tr.selected').removeClass('selected');
                $(nRow).addClass('selected');
            }

            return nRow;
        },
        "order": [[4, 'asc']]
    });

    // select row (highlight)
    $('#sumTable tbody').on('click', 'tr', function () {

        if ($(this).hasClass('odd') || $(this).hasClass('even')) {
            summary_table.$('tr.selected').removeClass('selected');
            $(this).addClass('selected');
        }

        var $temp = $("<input>");
        $("body").append($temp);
        $temp.val($('td:eq(3)', $(this)).text()).select();
        document.execCommand("copy");
        $temp.remove();
    });

    // Open details
    $('#sumTable').on('click', 'tbody a', function () {
        var $tr = $(this).closest('tr');
        var data = summary_table.row($tr).data();
        var note_id = data["Note_id"];
        localStorage.setItem("last_id", note_id);

        let url = $('#queryDetails').data('request-url');
        window.open(url.replace('_id_', note_id), "#top");
    });

    // Add event listener for opening and closing details
    $('#sumTable tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = summary_table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
            Set_Nested_Icon(tr, row.data()["Status"], true);
        }
        else {
            // Open this row
            row.child(format_tbl(row.data(), tr)).show();
            tr.addClass('shown');
            Set_Nested_Icon(tr, row.data()["Status"], false);
        }
    });

    // Double click event
    $('#sumTable tbody').on('dblclick', 'td', function () {
        if (!$(this).hasClass('details-control')) {

            var rowData = summary_table.row(this).data();
            var note_id = rowData["Note_id"];
            localStorage.setItem("last_id", note_id);

            let url = $('#queryDetails').data('request-url');
            window.open(url.replace('_id_', note_id), "#top");
        }
    });

    // Add excel and print buttons
    var buttons = new $.fn.dataTable.Buttons(summary_table, {

        buttons: [
            {
                extend: 'csv',
                "text": '<i class="fa fa-file-excel-o">&thinsp; Excel</i>',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
                }

            },
            {
                extend: 'print',
                "text": '<i class="fa fa-print">&thinsp; Print</i>',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
                }
            }
        ]
    }).container().appendTo($('#export_btns'));

    // search by value
    $("#searchVal").keyup(function () {
        summary_table.search(this.value).draw();
    });

    // filter by checked values
    $('input[type=checkbox]').change(function () {

        if ($(this).attr("id") === "all-checkbox") {

            var txtSearch = $("label[for='" + $(this).attr("id") + "']").text().trim();

            if ($(this).is(":checked")) {
                $('input[type=checkbox]').each(function () {
                    this.checked = true;
                    summary_table.columns(2).search('', true, false).draw();
                });
            } else {
                $('input[type=checkbox]').each(function () {
                    this.checked = false;
                    summary_table.columns(2).search(txtSearch, true, false).draw();
                });
            }
        }
        else {
            var arrSelected = [];

            $('input[type=checkbox]').each(function () {
                if ($(this).is(":checked")) {
                    arrSelected.push('(?=.*' + $("label[for='" + $(this).attr("id") + "']").text().trim() + ')');
                }
            });

            $('#all-checkbox').prop('checked', false);

            if (arrSelected.length === 0) {
                summary_table.columns(2).search('dummy', true, false, true).draw();
            }
            else {
                summary_table.columns(2).search(arrSelected.join('|'), true, false, true).draw();
            }
        }
    });

    //Populate clients drop down
    Get_Clients_Data();

    // Drop clients down select values
    $('#clients_ddl').change(function () {
        summary_table.columns(14).search($('#clients_ddl option:selected').val(), true, false, true).draw();
    });

    $('#dateFrom').on("dp.change", function (e) {
        summary_table.draw();
    });

    $('#dateTo').on("dp.change", function (e) {
        summary_table.draw();
    });

    $(window).focus(function () {
        var isRefreshed = localStorage.getItem("isSummaryRefreshed");
        if (isRefreshed === 'false') {
            summary_table.ajax.reload();
            localStorage.setItem("isSummaryRefreshed", 'true');
        }
    });
});

// Icons for open buttons 
function Set_Nested_Icon(nRow, status, isForExpand) {
    var url = "#";
    if (isForExpand) {
        if (status === "Complete") {
            url = $('#queryCompleteOpen').data('request-url');
        }
        else if (status === "In progress") {
            url = $('#queryInProgressOpen').data('request-url');
        }
        else if (status === "New found Dx") {
            url = $('#queryFoundOpen').data('request-url');
        }
        else if (status === "Not processed") {
            url = $('#queryNotProcOpen').data('request-url');
        }
    }
    else {
        if (status === "Complete") {
            url = $('#queryCompleteClose').data('request-url');
        }
        else if (status === "In progress") {
            url = $('#queryInProgressClose').data('request-url');
        }
        else if (status === "New found Dx") {
            url = $('#queryFoundClose').data('request-url');
        }
        else if (status === "Not processed") {
            url = $('#queryNotProcClose').data('request-url');
        }
    }
    $('td:eq(1)', nRow).css({ 'background': "url(" + url + ") no-repeat", 'background-position': 'center' });
}

// filter by date
$.fn.dataTableExt.afnFiltering.push(
    function (oSettings, aData, iDataIndex) {

        var cellDate = moment(aData[12]);

        if ($("#dateToVal").val().length > 0 && $("#dateFromVal").val().length > 0) {

            return cellDate.isBefore(moment($("#dateToVal").val())) && cellDate.isAfter(moment($("#dateFromVal").val()));
        }
        else if ($("#dateToVal").val().length > 0 && $("#dateFromVal").val().length == 0) {

            return cellDate.isBefore(moment($("#dateToVal").val()));
        }
        else if ($("#dateToVal").val().length == 0 && $("#dateFromVal").val().length > 0) {

            return cellDate.isAfter(moment($("#dateFromVal").val()));
        }
        else return true;
    });

// format inner table
function format_tbl(d, tr) {
    var arr = jQuery.parseJSON(d.InnerData);
    var html = '';
    //var numHcc = '';

    html = '<table class="table-striped table-hover border_ext" cellpadding="3" cellspacing="0" border="1" width="100%">' +
        '<thead style="background-color: #253949; color: white">' +
        '<tr>' +
        '<th width="10%">HCC</th>' +
        '<th width="10%">Dx</th>' +
        '<th width="80%">Description</th>' +
        '</tr>' +
        '</thead>';
    for (var i = 0; i < arr.length; i++) {

        html +=
            '<tr >' +
            '<td>' + arr[i].hcc_code + '</td>' +
            //(numHcc != arr[i].hcc_code ? '<td>' + arr[i].hcc_code + '</td>' : '<td></td>') +
            '<td>' + arr[i].dx_code + '</td>' +
            '<td>' + arr[i].description + '</td>' +
            '</tr>';
      //  numHcc = arr[i].hcc_code;
    }
    html += '</table>';

    return html;
}


// datetimepicker
$(function () {
    $('#dateFrom').datetimepicker({
        viewMode: 'days',
        format: 'MM/DD/YYYY',
        useCurrent: false
    });
    $('#dateTo').datetimepicker({
        viewMode: 'days',
        format: 'MM/DD/YYYY',
        useCurrent: false
    });
    $("#dateFrom").on("dp.change", function (e) {
        $('#dateTo').data("DateTimePicker").minDate(e.date);
    });
    $("#dateTo").on("dp.change", function (e) {
        $('#dateFrom').data("DateTimePicker").maxDate(e.date);
    });
});

// expand / collapse inner table
$(document).on('click', '.panel-heading span.clickable', function (e) {
    var $this = $(this);
    if (!$this.hasClass('panel-collapsed')) {
        $this.parents('.panel').find('.panel-body').slideUp();
        $this.addClass('panel-collapsed');
        $this.find('i').removeClass('glyphicon-chevron-up').addClass('glyphicon-chevron-down');
    } else {
        $this.parents('.panel').find('.panel-body').slideDown();
        $this.removeClass('panel-collapsed');
        $this.find('i').removeClass('glyphicon-chevron-down').addClass('glyphicon-chevron-up');
    }
});

//Get data for user table
function Get_Clients_Data() {
    return $.ajax({
        url: $('#queryGetClientsData').data('request-url'),
        type: 'GET',
        async: false,
        datatype: 'json',
        success: function (data) {
            AddClients(data);
            $('#clients_ddl').selectpicker('refresh');
        }
    });
}

function AddClients(objClient) {

    var $option = $("<option selected='selected'></option>");
    $("#clients_ddl").append($option);

    $.each(objClient, function (key, value) {
        $option = $("<option></option>", {
            "text": value.Client_Name,
            "value": value.Client_Id
        });
        $("#clients_ddl").append($option);
    });
}

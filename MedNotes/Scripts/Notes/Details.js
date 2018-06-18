
$(document).ready(function () {

    var arr_nlp = new Array();
    var note_id = $("#noteId").data("value");
    var uniData = Get_Unidentified_Data(note_id);
    Get_Person_Data(note_id);

    $(window).bind('storage', function (e) {
        
        //  console.log(e.originalEvent.key, e.originalEvent.newValue);
        $('input[id=linkTxt]').val(e.originalEvent.newValue);
        GetRowByDx(e.originalEvent.key, $('input[id=linkTxt]').val(), uniData, arr_nlp);
        localStorage.removeItem(e.originalEvent.key);

    });

    $('[data-toggle="tooltip"]').tooltip();   

    jQuery.fn.dataTable.Api.register('row.addByPos()', function (data, index) {

        //insert the row
        this.row.add(data).draw();

        ////move added row to desired index
        var rowCount = this.data().length - 1,
            obj_Values;

        for (var i = rowCount; i >= index; i--) {

           obj_Values = GetPreviousValues(i - 1);
           SetNextValues(i, obj_Values);
           SetEmptyRow(i - 1);
        }
        //refresh the current page
        this.page(0).draw(false);
    });

    // Creation of NLP Diagnosis TABLE (or user table)
    var tbl_NLP_Diagn = $('#tbl_NLP_Diagnosis').DataTable({
        "ajax": {
            "url": $('#queryGetNLPData').data('request-url'),
            "type": 'GET',
            "dataSrc": "",
            "data": {
                "note_id": note_id
            }
        },
        "dom": '<<t>p>',
        "pageLength": 8,
        "columns": [
            {
                "data": "Status",
                "render": function (data, type, full, meta) {
                    var html = '';
                    if (data.toString() == 'Rejected') {
                        html += '<label style="font-size:11.5px" for="' + meta["row"] + '_Accepted"><input type="radio" id="' + meta["row"] + '_Accepted" name="' + meta["row"] + '" class="singleRadio" value="Accepted" unchecked>Accepted</label>';
                        html += '<label style="font-size:11.5px" for="' + meta["row"] + '_Rejected"><input type="radio" id="' + meta["row"] + '_Rejected" name="' + meta["row"] + '" class="singleRadio" value="Rejected" checked>Rejected</label>';
                    }
                    else if (data.toString() == 'Accepted') {
                        html += '<label style="font-size:11.5px" for="' + meta["row"] + '_Accepted"><input type="radio" id="' + meta["row"] + '_Accepted" name="' + meta["row"] + '" class="singleRadio" value="Accepted" checked>Accepted</label>';
                        html += '<label style="font-size:11.5px" for="' + meta["row"] + '_Rejected"><input type="radio" id="' + meta["row"] + '_Rejected" name="' + meta["row"] + '" class="singleRadio" value="Rejected" unchecked>Rejected</label>';
                    }
                    else {
                        $("#fancy-checkbox-success").prop('checked', false);
                        html += '<label style="font-size:11.5px" for="' + meta["row"] + '_Accepted"><input type="radio" id="' + meta["row"] + '_Accepted" name="' + meta["row"] + '" class="singleRadio" value="Accepted" unchecked>Accepted</label>';
                        html += '<label style="font-size:11.5px" for="' + meta["row"] + '_Rejected"><input type="radio" id="' + meta["row"] + '_Rejected" name="' + meta["row"] + '" class="singleRadio" value="Rejected" unchecked>Rejected</label>';
                    }
                    
                    return html;
                }
            },
            {
                "data": "Confidence",
                "render": function (data, type, full, meta) {
                    var descr = full["Description"].length > 33 ? full["Description"].substring(0, 33) +
                                                                    '<a href="#" class="btn_show_ext" id="btn_show" data-toggle="tooltip"  title="Show description">...</a>'
                                                                : full["Description"];
                    var html = '';

                    html += '<span class="fakeCell_conf">' + full["Confidence"] + '</span><span class="fakeCell_icd10">&nbsp;' + full["ICD10"] + '</span>' +
                        '<span>&nbsp;' + descr + '</span>' +
                        '<input  id="rej_reason_input" placeholder="Rejected reason..." type="text" class="form-control" value="' + full["Rej_Reason"] + '">' +
                        '<input id="valid_cond_id" type="text" hidden  value="' + full["Valid_Condition_Id"] + '"><input id="rej_cond_id" hidden  type="text"  value="' + full["Rej_Condition_Id"] + '">' +
                        '<input id="full_descr" type="text" hidden  value="' + full["Description"] + '">' +
                        '<input name="rel_nlp_txt" type="text" hidden value="' + full["Related_text"] + '">' +
                        '<input name="rel_nlp_start" type="text" hidden value="' + full["Related_start"] + '">' +
                        '<input name="rel_nlp_end" type="text" hidden value="' + full["Related_end"] + '">' +
                        '<input name="rel_nlp_snomed" type="text" hidden  value="' + full["Snomed_code"] + '">';
                    return html;
                }
            }
        ],
        "initComplete": function (settings, json) {
            // nlp array population    
            NLPArrayPopulation(tbl_NLP_Diagn, arr_nlp);
        },
        "rowCallback": function (row, data, index) {
            if (data["Status"] === "Rejected") {
                $(row).find("label[for*='_Rejected']").css('color', 'red');
                $(row).find("input[id='rej_reason_input']").show();
            } else {
                $(row).find("input[id='rej_reason_input']").hide();
            }
        }
        , "ordering": false

    });

    //  Creation of Unidentified Added Diagnosis TABLE (or user table)
    var tbl_User_Diagn = $('#tbl_User_Diagnosis').DataTable({

        "dom": '<<t>>',
        "columns": [
            {
                "name": 'a',
                "render": function (data, type, row) {
                    return  '<select id="code_select" class="form-control input_margin_ext" data-live-search="true"></select>' +
                            '<form class="form-inline"> <input class="form-control select_ext input_margin_ext" name="how_ident" type="text" placeholder="How identified...">' +
                            '<button id="btn_add" class="btn btn-default glyphicon glyphicon-plus btn_margin_ext" data-toggle="tooltip"  title="Add selected" type="button"></button>' +
                            '<button id="btn_del" class="btn btn-default glyphicon glyphicon-trash btn_margin_ext" data-toggle="tooltip"  title="Remove row" type="button"></button></form>' +
                            '<input name="rel_txt" type="text" hidden>' +
                            '<input name="rel_start" hidden type="text">' +
                            '<input name="rel_end" hidden type="text">';
                }
            }
        ],
        drawCallback: function () {
            $('#code_select').select2({
                minimumInputLength: 2,
                minimumResultsForSearch: 10,
                overflow: scroll,
                placeholder: "Select value...",
                ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
                    url: $('#queryGetICD10').data('request-url'),
                    type: 'GET',
                    dataType: 'json',
                    data: function (params) {

                        var queryParameters = {
                            query_val: params.term, // search term
                            note_id: note_id,
                        }
                        return queryParameters;
                    },
                    processResults: function (data) {
                        return {
                            results: $.map(data, function (item) {
                                return {
                                    text: item.ICD10_code + ' ' + item.ICD10_description,
                                    id: item.ICD10_code
                                }
                            })
                        };
                    }
                }
            });
        },
        "ordering": false

    });

    // Drop down select values
    $('#tbl_User_Diagnosis tbody').on('change', 'td', function () {

        var $row = $(this).closest('tr');
        var $td_code = $row.find("#code_select option:selected").val();
        var $row_zero = $('#tbl_User_Diagnosis').find('tbody tr:eq(0)');

        var succsess = false;

        if ($td_code != '')
            succsess = true;

        if (succsess && $row.index() === 0 && tbl_User_Diagn.data().length < 11) {
            var table = $("#tbl_User_Diagnosis").DataTable();
            table.row.addByPos(['', '-', ''], 1);
        } else if (!succsess && $(this).index() != 2 && $row.index() > 0) {
            SetEmptyRow($row.index());
        }

        if (tbl_User_Diagn.data().length === 11) {
           
            $row_zero.find("#code_select").select2({
                placeholder: "Maximum 10 rows"
            });
            $row_zero.find("#code_select").prop("disabled", true);
            $row_zero.find(':input[name="how_ident"]').prop("disabled", true);
        }
        // trick for the div scroll bar
        $row_zero.find("#code_select").select2('open').select2('close');

    });

    // tbl_NLP_Diagnosis. Select row (highlight row + highlight text) 
    $('#tbl_NLP_Diagnosis tbody').on('click', 'tr', function () {

        var $row = $(this).closest('tr');
        var start_pos = $row.find(':input[name="rel_nlp_start"]').val()

        $('#frame').contents().find("a").each(function () {
            if ($(this).attr("data-start") == start_pos)
                $(this).css("background-color", "LightGray");
            else
                $(this).css("background-color", "");
        });

        if ($(this).hasClass('odd') || $(this).hasClass('even')) {
            tbl_NLP_Diagn.$('tr.selected').removeClass('selected');
            tbl_User_Diagn.$('tr.selected').removeClass('selected');
            $(this).addClass('selected');
        }
        
    });


    // tbl_User_Diagnosis. Select row (highlight row + highlight text) 
    $('#tbl_User_Diagnosis tbody').on('click', 'tr', function () {

        var $row = $(this).closest('tr');

        if ($row.index() != 0) {

            var $row = $(this).closest('tr');
            var start_pos = $row.find(':input[name="rel_start"]').val()

            $('#frame').contents().find("a").each(function () {
                if ($(this).attr("data-start") === start_pos)
                    $(this).css("background-color", "LightGray");
                else
                    $(this).css("background-color", "");
            });
        }

        if ($(this).hasClass('odd') || $(this).hasClass('even')) {
            tbl_NLP_Diagn.$('tr.selected').removeClass('selected');
            tbl_User_Diagn.$('tr.selected').removeClass('selected');
            $(this).addClass('selected');
        }
    });

    //Add new row into user table
    AddUsrTblRow(tbl_User_Diagn, uniData, note_id);

    // radiobutton click event
    $("#tbl_NLP_Diagnosis").delegate("input[type='radio']", "change", function () {

        var status = $(this).val();
        var $row = $(this).closest('tr');

        var start_pos = $row.find(':input[name="rel_nlp_start"]').val()
        var $input_ctrl = $row.find("input[id='rej_reason_input']")

        status === "Rejected" ? $row.find("label[for*='_Rejected']").css('color', 'red') :
                                 $row.find("label[for*='_Rejected']").css('color', 'black');

        status === "Accepted" ? $input_ctrl.hide() : $input_ctrl.show();

        var id_param = $row.find("input[type='radio']").attr("name");

        arr_nlp.forEach(function (entry) {
            if (entry.key == id_param)
                entry.value.Status = status;
        });

        var is_rejected = true;

        $('#frame').contents().find("a").each(function () {
            var $a = $(this)
            var a_start = $(this).attr("data-start");

            if (a_start === start_pos) {
                if (status === "Accepted")
                    $a.css("color", "black");
                else {
                    arr_nlp.forEach(function (entry) {
                        if (entry.value.Related_start == start_pos && entry.value.Status != "Rejected") {
                            $a.css("color", "black");
                            is_rejected = false;
                            return false;
                        }
                    });
                    if (is_rejected == true) {
                        $a.css("color", "red");
                    }
                }
            }
        });

        SetUpCompleteCheck(arr_nlp);

    });

    // change rejected reason event
    $("#tbl_NLP_Diagnosis").delegate("input[id='rej_reason_input']", "change", function () {

        var reason = $(this).val();
        var $row = $(this).closest('tr');
       
        var id_param = $row.find("input[type='radio']").attr("name");

        arr_nlp.forEach(function (entry) {
            if (entry.key == id_param)
                entry.value.Rej_Reason = reason;
        });

    });

    // Add button click value
    $('#tbl_User_Diagnosis tbody').on('click', 'td button', function () {
        var $row = $(this).closest('tr');

        var iframe = document.getElementById("frame");
        var sel_obj = getIframeSelectionText(iframe);

        if ($(this).prop("id") === 'btn_add') {
            if (sel_obj != null) {

                RemoveRefTag($row.find(':input[name="rel_txt"]').val());

                $row.find(':input[name="rel_txt"]').val(sel_obj.text);
                $row.find(':input[name="rel_start"]').val(sel_obj.start);
                $row.find(':input[name="rel_end"]').val(sel_obj.end);
                // console.log("start: " + sel_obj.start + "; end: " + sel_obj.end + "; text:" + sel_obj.text);
            }
        } else {

            RemoveRefTag($row.find(':input[name="rel_txt"]').val());
            tbl_User_Diagn.row($(this).parents('tr')).remove().draw();
        }
    });

    // SEARCH 
    $("#btn_doc_search").click(function () {
        SearchInFiles($('#srch_val').val(), $('#patient_id').val());
        $("#docs_txt_header").text('Documents');
        $('#DocsModal').modal("show");
    });

    // Open searchable details
    // Open details
    $('#tbl_patient_files').on('click', 'tbody a', function () {
        var $tr = $(this).closest('tr');
        var data = $('#tbl_patient_files').DataTable().row($tr).data();
        note_id = data["Note_id"];
        localStorage.setItem("last_id", note_id);

        let url = $('#queryDetails').data('request-url');
        window.open(url.replace('_id_', note_id), "#top");
    });


    $('#tbl_NLP_Diagnosis tbody').on('click', 'td a', function () {

        if ($(this).prop("id") === 'btn_show') {
            var $row = $(this).closest('tr');
            $("#txt_area").val($row.find(':input[id="full_descr"]').val());
            $("#txt_header").text($row.find('.fakeCell_icd10').text());
            $('#DescModal').modal("show");
        }
    });

    $("#btnCancel").click(function () {
        localStorage.setItem("last_id", note_id);
        window.close();
    });

    $("#btnOK").click(function () {
        localStorage.setItem("last_id", note_id);
        localStorage.setItem("isSummaryRefreshed", 'false');
        $('#frame').contents().find("a").each(function () {
           $(this).css("background-color", "");
        });
        StoreDoc(note_id, arr_nlp);
    });

});

// nlp array population  
function NLPArrayPopulation(tbl_NLP_Diagn, arr_nlp) {

    tbl_NLP_Diagn.rows().every(function () {
        arr_nlp.push({ key: this.index(), value: this.data() });
    });
}

// search value in html files
function SearchInFiles(text, patient_id) {
    // Patient files table
    var tbl_Files = $('#tbl_patient_files').DataTable({
        "ajax": {
            "url": $('#searchInHtmlFiles').data('request-url'),
            "dataSrc": "",
            "data": {
                "search_val": text,
                "patient_id": patient_id
            }
        },
        "dom": '<<t>lp>',
        "columns": [
            {
                "data": "Note_id",
                "visible": false
            },
            {
                "data": "File_Name",
                "render": function (data, type, full, meta) {
                    return '<a href="#">' + data + '</a>';
                }
            },
            {
                "data": "File_date",
            },
            {
                "data": "Num_entries",
            }
        ],
        destroy: true,
        searching: false
    });
}

// remove <a/> tag from old found and non valid deseases
function RemoveRefTag(cur_rel_txt) {
    if (cur_rel_txt != null && cur_rel_txt != undefined) {
        $('#frame').contents().find("a").each(function () {
            if ($(this).text() === cur_rel_txt)
                $(this).replaceWith(function () { return this.innerHTML; });
        });
    }
}

//Get ICD10 values for DDL
function Get_ICD10_Data(note_id) {
    return $.ajax({
        url: $('#queryGetICD10').data('request-url'),
        type: 'GET',
        data: {
            note_id: note_id
        },
        async: false
    }).responseJSON;
}

//Get data for user table
function Get_Unidentified_Data(note_id) {
    return $.ajax({
        url: $('#queryGetUnidentifiedData').data('request-url'),
        type: 'GET',
        data: {
            note_id: note_id
        },
        async: false
    }).responseJSON;
}

//Get peson data for the hesder
function Get_Person_Data(note_id) {
    return $.ajax({
        url: $('#queryGetPersonData').data('request-url'),
        type: 'GET',
        data: {
            note_id: note_id
        },
        datatype: 'json',
        success: function (data) {
            AddPerson(data);
        }
    });
}

//person header values
function AddPerson(objPerson) {

    $.each(objPerson, function (key, value) {
        $("#patient_id").val(value.Person_Id);
        $("#provider").val(value.Provider_Name);
        $("#person").val(value.Person_Name);
        $("#mrn").val(value.MRN);
        $("#dob").val(value.DOB);
        $("#age").val(value.Age);
        $("#gender").val(value.Gender);
        $("#race").val(value.Race);
        $("#note_date").val(value.NoteDate);
    });
}

// <adding line values in a loop to add a new line> (start)
function GetPreviousValues(row_idx) {

    var $row_prev = $('#tbl_User_Diagnosis').find('tbody tr:eq(' + row_idx + ')');

    var row_id = $row_prev.find("#code_select option:selected").val();
    var row_ddl_text = $row_prev.find("#code_select option:selected").text();
    var row_ident = $row_prev.find(':input[name="how_ident"]').val();
    var row_rel_txt = $row_prev.find(':input[name="rel_txt"]').val();
    var row_rel_start = $row_prev.find(':input[name="rel_start"]').val();
    var row_rel_end = $row_prev.find(':input[name="rel_end"]').val();

    var obj = {
        rowId: row_id,
        rowDDLText: row_ddl_text,
        rowIdent: row_ident,
        rowRelText: row_rel_txt,
        rowRelStart: row_rel_start,
        rowRelEnd: row_rel_end,
    };

    return obj;
}

function SetNextValues(row_idx, obj_Values) {

    var $row_next = $('#tbl_User_Diagnosis').find('tbody tr:eq(' + row_idx + ')');
    var option = new Option(obj_Values.rowDDLText, obj_Values.rowId, true);
    var $select = $row_next.find('#code_select');
    $select.find('option').remove();

    $select.append(option);
    $select.select2().val(obj_Values.rowId).trigger("change");
    $select.select2('destroy'); 

    $row_next.find('button').show();
    $row_next.find(':input[name="how_ident"]').show();
    $row_next.find(':input[name="how_ident"]').val(obj_Values.rowIdent);
    $row_next.find(':input[name="rel_txt"]').val(obj_Values.rowRelText);
    $row_next.find(':input[name="rel_start"]').val(obj_Values.rowRelStart);
    $row_next.find(':input[name="rel_end"]').val(obj_Values.rowRelEnd);
}

function SetEmptyRow(row_idx) {

    var $row_empty = $('#tbl_User_Diagnosis').find('tbody tr:eq(' + row_idx + ')');
    $row_empty.find('button').hide();
    
    $row_empty.find('option').remove();
    $row_empty.find(':input[name="how_ident"]').val('');
    $row_empty.find(':input[name="how_ident"]').hide();
    $row_empty.find(':input[name="rel_txt"]').val('');
    $row_empty.find(':input[name="rel_start"]').val('');
    $row_empty.find(':input[name="rel_end"]').val('');
}

function AddUsrTblRow(usrTbl, uniData, note_id) {

    if (uniData.length < 10) {
        usrTbl.row.add(['', '-', '-']).draw();
        SetEmptyRow(0);
    }

    $.each(uniData, function (key, value) {

        usrTbl.row.add(['', '-', '-']).draw();

        var $row_new = $("#tbl_User_Diagnosis tr:last");
        var $select = $row_new.find('#code_select');
        var option = new Option(value.ICD10 + ' ' + value.Description, value.ICD10, true);

        $select.append(option);
        $select.select2().val(value.ICD10).trigger("change");
        $select.select2('destroy'); 

        $row_new.find(':input[name="how_ident"]').val(value.How_Ident);
        $row_new.find(':input[name="rel_txt"]').val(value.Related_text);
        $row_new.find(':input[name="rel_start"]').val(value.Related_start);
        $row_new.find(':input[name="rel_end"]').val(value.Related_end);
    });
}

// </ adding line values in a loop to add a new line> (stop)

function GetId(name) {
    return (location.search.split(name + '=')[1] || '').split('&')[0];
}

//get ifrme selected text
function getIframeSelectionText(iframe) {
    // return values
    var start = 0, end = 0, text_val;
    // function values
    var sel, range, priorRange;
    var win, doc = iframe.contentDocument;

    if (doc) {
        win = doc.defaultView;
    } else {
        win = iframe.contentWindow;
        doc = win.document;
    }
  

    if (typeof win.getSelection != "undefined" && win.getSelection()) {

        if (win.getSelection().rangeCount > 0) {

            try {

                text_val = win.getSelection().toString();
                range = win.getSelection().getRangeAt(0);
                priorRange = range.cloneRange();
                priorRange.selectNodeContents(iframe.contentDocument);
                priorRange.setEnd(range.startContainer, range.startOffset);
                start = priorRange.toString().length;
                end = start + range.toString().length;

                var myTag = iframe.contentDocument.createElement("a");
                myTag.setAttribute("id", "id_add");
                myTag.setAttribute("onclick", "linkClicker('" + start + "')");
                myTag.setAttribute("data-start", start);
                myTag.setAttribute("data-end", end);
                myTag.setAttribute("data-toggle", "tooltip");
                myTag.setAttribute("data-placement", "bottom");
                myTag.setAttribute("style", "text-decoration: underline; font-weight:bold; color:#0BABC8; cursor: pointer;");
                myTag.setAttribute("title", "Dx Added");

                range.surroundContents(myTag);
            } catch (e) {
                return null;
            }

        } else {
            return null;
        }
    }

    return {
        start: start,
        end: end,
        text: text_val
    };
}

function GetRowByDx(key, start_pos, uniData, arr_nlp) {

    $("#tbl_NLP_Diagnosis").find('tr.selected').removeClass('selected');
    $("#tbl_User_Diagnosis").find('tr.selected').removeClass('selected');

    var nlp_table = $('#tbl_NLP_Diagnosis').DataTable();
    var nlp_index = -1;

    arr_nlp.forEach(function (entry) {
        if (entry.value.Related_start == start_pos) {
            nlp_index = entry.key;
            if (nlp_index > 7) {
                nlp_table.page(Math.floor(nlp_index / 8)).draw(false);
            } else {
                nlp_table.page(Math.floor(0)).draw(false);
            }
        }
    });

    if (nlp_index != -1) {
        var tableRow = $('#tbl_NLP_Diagnosis tr td').filter(function () {
            return $(this).find(':input[name="rel_nlp_start"]').val() == start_pos;
        }).parent('tr').addClass('selected');
    }
    else {
        var tableRow = $('#tbl_User_Diagnosis tr td').filter(function () {
            return $(this).find(':input[name="rel_start"]').val() == start_pos.toString();
        }).parent('tr').addClass('selected');
    }


    $('#frame').contents().find("a").each(function () {
        if ($(this).attr("data-start") === start_pos)
            $(this).css("background-color", "LightGray");
        else
            $(this).css("background-color", "");
    });
}

// Set up Copmlete check box value
function SetUpCompleteCheck(arr_nlp) {
    if (IsStatusesTurnedOn($("#tbl_NLP_Diagnosis"), arr_nlp)) {
        $("#fancy-checkbox-success").prop('checked', true);
    } else {
        $("#fancy-checkbox-success").prop('checked', false);
    }
}

//check if all Statuses is turned on
function IsStatusesTurnedOn(tblObj, arr_nlp) {

    var is_checked = true;

    arr_nlp.forEach(function (entry) {

        if (entry.value.Status != "Rejected" && entry.value.Status != "Accepted") {
            is_checked = false;
            return false;
        }

    });

    return is_checked;
}


// Save data function
function StoreDoc(note_id, arr_nlp) {
    $.ajax({
        url: $('#saveDataOnServer').data('request-url'),
        type: 'POST',
        async: true,
        data: JSON.stringify(
            {
                NLPData: JSON.stringify(NLP_Table_To_JSON($("#tbl_NLP_Diagnosis"), arr_nlp)),
                UserData: JSON.stringify(Added_Table_To_JSON($("#tbl_User_Diagnosis"))),
                note_id: note_id,
                is_complete: $("#fancy-checkbox-success").is(':checked') ? 1 : 0,
                html_text: $('#frame').contents().find("html").html()
            }),
        contentType: 'application/json',
        success: function () {
            window.close();
        }
    });
}

// Read added diagnosis table data for saving
function Added_Table_To_JSON(tblObj) {

    var data = [];

    var $rows = $(tblObj).find("tbody tr").each(function (index) {

        var related_text = $(this).find(':input[name="rel_txt"]').val();

        if (related_text === '' || related_text === undefined) {
            return true; //This is same as 'continue'
        }

        var $cells = $(this).find("td");
        var row_id = $(this).index();

        $cells.each(function (cellIndex) {
            //scan added diagnosis table

            if (cellIndex === 0) {
                var ddl_val = $(this).find('option:selected').val();
                if (ddl_val != undefined && ddl_val != '') {
                    // create data object
                    data[index] = {};
                    //get drop down list selected value
                    data[index]["ICD10"] = ddl_val; // add name:value pairs into object
                    //get "How identified"
                    data[index]["How_Ident"] = $(this).find(':input[name="how_ident"]').val();
                    //get "Related_text"
                    data[index]["Related_text"] = $(this).find(':input[name="rel_txt"]').val();
                    //get "Related_start" position
                    data[index]["Related_start"] = $(this).find(':input[name="rel_start"]').val();
                    //get "Related_end" position
                    data[index]["Related_end"] = $(this).find(':input[name="rel_end"]').val();
                }
                else
                    return false; //This is same as 'break'
                //return true; //This is same as 'continue'
            }
        });
    });
    return data;
}

// Read nlp table data for saving
function NLP_Table_To_JSON(tblObj, arr_nlp) {

    var data = [];

    arr_nlp.forEach(function (entry) {

        if (entry.value.Status != "Rejected" && entry.value.Status != "Accepted") {
            is_checked = false;
            return false;
        }

    });

    arr_nlp.forEach(function (entry) {

        var index = entry.key;

        if (entry.value.Status != "Rejected" && entry.value.Status != "Accepted") {
            return true; //This is same as 'continue'
        }
        else {
            data[index] = {};
            data[index]["Status"] = entry.value.Status;
            data[index]["Confidence"] = entry.value.Confidence;
            data[index]["ICD10"] = entry.value.ICD10;
            data[index]["Description"] = entry.value.Description;
            data[index]["Rej_Reason"] = entry.value.Rej_Reason;
            data[index]["Valid_Condition_Id"] = entry.value.Valid_Condition_Id;
            data[index]["Rej_Condition_Id"] = entry.value.Rej_Condition_Id;
            //get "Related_text"
            data[index]["Related_text"] = entry.value.Related_text;
            //get "Related_start" position
            data[index]["Related_start"] = entry.value.Related_start;
            //get "Related_end" position
            data[index]["Related_end"] = entry.value.Related_end;
            // get "Snomed_code"
            data[index]["Snomed_code"] = entry.value.Snomed_code;
        }
    });

    return data;
}


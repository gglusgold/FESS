(function () {
    // your page initialization code here
    // the DOM will be available here




    $("#btn_iniciar").click(function(){
		$("#login").modal("toggle")
	});

    $("#btn_registrarse").click(function(){
		$("#registrarse").modal("toggle")
	});



    $('#btn_iniciar').on('click', function (e) {
        $('#btn_registrarse').popover('hide');
    });

    $('#btn_cerrar').on('click', function (e) {
        Cookies.remove("listado");
    });
    

    $('#btn_registrarse').on('click', function (e) {
        $('#btn_iniciar').popover('hide');
    });

    $("#usuario,#pass").keypress(function () {
        if ($("#divError").is(":visible"))
            $("#divError").hide("slow");
    })

    var error = Cookies.get('Error');
    var info = Cookies.get('Info');
    var mensaje = Cookies.get('Mensaje');

    if ((error !== undefined && error !== '') || (info !== undefined && info !== '') || (mensaje !== undefined && mensaje !== '')) {
        $("#login").modal("toggle")
        $('#btn_iniciar').popover('show');
        if (error !== "")
            $("#divError").addClass("alert-danger");

        if (info !== "")
            $("#divError").addClass("alert-info");

        if (mensaje !== "")
            $("#divError").addClass("alert-success");

        $("#divError").show("slow");
        $("#sError").text(error || info || mensaje);

        Cookies.remove('Error');
        Cookies.remove('Info');
        Cookies.remove('Mensaje');
    }


    var password = document.getElementById("password"), confirm_password = document.getElementById("confirmpassword");
    function validatePassword() {
        if (password.value !== confirm_password.value) {
            confirm_password.setCustomValidity("Las contraseñas no coinciden");
        } else {
            confirm_password.setCustomValidity('');
        }
    }

    password.onchange = validatePassword;
    confirm_password.onkeyup = validatePassword;

    $(document).on("submit", "#login-form", function (e) {
        $("#login-form").find("input[type='submit']").button('loading');
        $("#login-form").addHidden('url', window.location.pathname);
        return true;
    });

    $(document).on("submit", "#register-form", function (e) {
        $("#register-form").find("input[type='submit']").button('loading');
        $("#register-form").addHidden('url', window.location.pathname);
        return true;
    });

    

})();
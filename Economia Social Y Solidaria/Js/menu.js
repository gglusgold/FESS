(function () {
    // your page initialization code here
    // the DOM will be available here




    $("#btn_iniciar").popover({
        html: true,
        content: function () {
            return $("#login").html();
        },
        title: "Iniciar Sesion",
        placement: "bottom"
    });

    $("#btn_registrarse").popover({
        html: true,
        content: function () {
            return $("#registrarse").html();
        },
        title: "Registrarse",
        placement: "bottom"
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
        $("#login-form").addHidden('url', window.location.pathname).submit();
    });

    $(document).on("submit", "#register-form", function (e) {
        $("#register-form").addHidden('url', window.location.pathname).submit();
    });

    

})();
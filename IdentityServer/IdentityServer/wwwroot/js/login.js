(function ($) {
    'use strict';
    $(function () {

        $('.cpf').inputmask("999.999.999-99");
        $('.celular').inputmask("(99)99999-9999");
        $('#spinnerSubmit').hide();
        //$("#popupTermosECondicoes").modal('hide');

        SelecionaTipoDeLogin();

        $('#rdoLoginPorEmail').on("click", function () {
            AlterarTipoDeLogin("email");
        });

        $('#rdoLoginPorCPF').on("click", function () {
            AlterarTipoDeLogin("cpf");
        });

        $('.btnVoltar').on("click", function () {
            GoBackBrowserHistory();
        });

        $('#lnkPopUpTermosECondicoes').on("click", function () {
           /* alert('Abrir PopUp Contendo o a partial _TermosECondicoes.cshtml');*/
            $("#popupTermosECondicoes").modal();
        });

        // Ao clicar 
        $('#registerSubmit').on("mouseup", function (elem) {
            RegistrarUsuarioClick();
        });

        function RegistrarUsuarioClick() {
            // Mostra spinner no botão
            $('#spinnerSubmit').show();
        }

        /// Seleciona o Tipo de login adequado de acordo com o radio button atualmente selecionado na tela.
        function SelecionaTipoDeLogin() {
            if ($('#rdoLoginPorEmail').is(':checked')) {
                AlterarTipoDeLogin("email");
            }
            else {
                AlterarTipoDeLogin("cpf");
            }
        }

        function AlterarTipoDeLogin(tipoLogin) {
            if (tipoLogin === 'email') {
                $('#divLoginPorCPF').hide();
                $('#divLoginPorEmail').show();
                $('#txtLoginCPF').val('');// Limpa o CPF para não executar client validation
            } else if (tipoLogin === 'cpf') {
                $('#divLoginPorCPF').show();
                $('#divLoginPorEmail').hide();
                $('#txtLoginEmail').val('');// Limpa o Email para não executar client validation
            }
            else {
                alert('Tipo de Login deve ser "E-mail" ou "CPF"');
            }
        }

        function GoBackBrowserHistory() {
            history.back();
            return false;
        }


    });
})(jQuery);
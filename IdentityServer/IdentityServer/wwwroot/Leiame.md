#Organização de CSS e JS
Este projeto utiliza a biblioteca https://github.com/madskristensen/BundlerMinifier para fazer bundle e minificar arquivos CSS e JS
Isso é importante para diminuir ao maximo o tempo de carregamento da pagina e a quantidade de download que o browser faz.

Em desenvolvimento você pode usar os scripts sem minificar para facilitar o debug mas em produção utilize sempre os scripts minificados.
Veja como os scripts são inseridos no layout principal (_Layout).

Sempre que adicionar um novo arquivo de CSS ou JS, lembre de incluí-lo no arquivo bundleconfig.json para que ele fique disponivel em produção.

#IMPORTANTE
Neste projeto a execução de scripts e CSS inline são bloqueados para aumentar a segurança.
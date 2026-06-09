# Sistema de Monitoramento de Detritos Espaciais

## Visão Geral

Este projeto foi desenvolvido para a Global Solution com o objetivo de monitorar detritos espaciais, registrar satélites e sensores, gerar alertas de risco e disponibilizar informações por meio de uma API REST.

A proposta busca demonstrar como técnicas de engenharia de software podem ser aplicadas ao problema crescente do lixo espacial, contribuindo para a segurança de missões, satélites comerciais e serviços dependentes da infraestrutura espacial.

## Objetivos

- Registrar e monitorar detritos espaciais.
- Gerenciar satélites e sensores de observação.
- Gerar alertas de possíveis colisões.
- Disponibilizar dados por meio de endpoints REST.
- Manter histórico de eventos utilizando DateTime.
- Aplicar conceitos de POO, Clean Architecture e boas práticas de desenvolvimento.

## Arquitetura

O projeto foi organizado em quatro camadas:

- Domain: entidades, interfaces e regras centrais.
- Application: serviços, DTOs e casos de uso.
- Infrastructure: Entity Framework Core, SQLite, JWT e repositórios.
- API: controllers, middleware, autenticação e Swagger.

## Requisitos Atendidos

- Programação Orientada a Objetos
- Herança, abstração e polimorfismo
- Interfaces e Injeção de Dependência
- DTOs e Value Objects
- Tratamento de exceções
- Banco de dados SQLite
- API REST
- Autenticação JWT
- Autorização
- CORS
- Swagger/OpenAPI
- Testes unitários

## Execução

cd src/SpaceDebrisMonitor.API
dotnet run

Após a inicialização, a documentação da API estará disponível no Swagger.

## Contexto da Global Solution

O sistema foi projetado para apoiar o monitoramento de lixo espacial e fornecer informações relevantes para operadores de satélites, organizações governamentais e empresas do setor aeroespacial.

using Dica33.TestesSnapshotComVerify;
using Xunit;

namespace Dica33.TestesSnapshotComVerify.Tests;

public class HtmlSnapshotTests
{
    [Fact]
    public async Task ShouldSnapshotUserReport()
    {
        // Arrange
        var generator = new ReportGenerator();

        // Act
        var html = await generator.GenerateUserReportAsync(1);

        // Assert
        await Verify(html)
            .ScrubLinesContaining("Data de geração:", "Relatório gerado pelo sistema em"); // Remove timestamps
    }

    [Fact]
    public Task ShouldSnapshotProductCard()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Smartphone Samsung Galaxy",
            Price = 1299.99m,
            Category = "Electronics"
        };

        var html = GenerateProductCardHtml(product);

        // Act & Assert
        return Verify(html);
    }

    [Fact]
    public Task ShouldSnapshotOrderInvoice()
    {
        // Arrange
        var invoice = GenerateOrderInvoiceHtml();

        // Act & Assert
        return Verify(invoice)
            .ScrubLinesContaining("Gerado em:"); // Remove timestamp
    }

    [Fact]
    public Task ShouldSnapshotEmailTemplate()
    {
        // Arrange
        var emailHtml = GenerateWelcomeEmailHtml("João Silva", "joao@email.com");

        // Act & Assert
        return Verify(emailHtml);
    }

    [Fact]
    public Task ShouldSnapshotDashboardWidget()
    {
        // Arrange
        var widgetHtml = GenerateDashboardWidgetHtml();

        // Act & Assert
        return Verify(widgetHtml);
    }

    [Fact]
    public Task ShouldSnapshotErrorPage()
    {
        // Arrange
        var errorPageHtml = GenerateErrorPageHtml(404, "Página não encontrada");

        // Act & Assert
        return Verify(errorPageHtml);
    }

    private static string GenerateProductCardHtml(Product product)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Produto - {product.Name}</title>
    <style>
        .product-card {{
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 16px;
            margin: 16px;
            max-width: 300px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .product-image {{
            width: 100%;
            height: 200px;
            background-color: #f5f5f5;
            border-radius: 4px;
            margin-bottom: 12px;
        }}
        .product-title {{
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 8px;
        }}
        .product-category {{
            color: #666;
            font-size: 14px;
            margin-bottom: 8px;
        }}
        .product-price {{
            font-size: 20px;
            font-weight: bold;
            color: #e74c3c;
            margin-bottom: 16px;
        }}
        .product-actions {{
            display: flex;
            gap: 8px;
        }}
        .btn {{
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
        }}
        .btn-primary {{
            background-color: #3498db;
            color: white;
        }}
        .btn-secondary {{
            background-color: #95a5a6;
            color: white;
        }}
    </style>
</head>
<body>
    <div class=""product-card"">
        <div class=""product-image""></div>
        <div class=""product-title"">{product.Name}</div>
        <div class=""product-category"">Categoria: {product.Category}</div>
        <div class=""product-price"">R$ {product.Price:F2}</div>
        <div class=""product-actions"">
            <button class=""btn btn-primary"">Comprar</button>
            <button class=""btn btn-secondary"">Favoritar</button>
        </div>
    </div>
</body>
</html>";
    }

    private static string GenerateOrderInvoiceHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Nota Fiscal - Pedido #1001</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { text-align: center; border-bottom: 2px solid #333; padding-bottom: 20px; }
        .company-name { font-size: 24px; font-weight: bold; }
        .invoice-info { margin: 20px 0; }
        .customer-info { margin: 20px 0; }
        .items-table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .items-table th, .items-table td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        .items-table th { background-color: #f2f2f2; }
        .total { font-weight: bold; font-size: 18px; text-align: right; }
        .footer { margin-top: 40px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class=""header"">
        <div class=""company-name"">E-Commerce Ltda</div>
        <div>CNPJ: 12.345.678/0001-90</div>
        <div>Rua das Empresas, 123 - São Paulo, SP</div>
    </div>
    
    <div class=""invoice-info"">
        <h2>Nota Fiscal Eletrônica</h2>
        <p><strong>Número:</strong> 000.001.001</p>
        <p><strong>Série:</strong> 1</p>
        <p><strong>Data de Emissão:</strong> 15/01/2024</p>
        <p><strong>Pedido:</strong> #1001</p>
    </div>
    
    <div class=""customer-info"">
        <h3>Dados do Cliente</h3>
        <p><strong>Nome:</strong> João Silva</p>
        <p><strong>CPF:</strong> 123.456.789-00</p>
        <p><strong>Endereço:</strong> Rua das Flores, 456 - São Paulo, SP - 01234-567</p>
    </div>
    
    <table class=""items-table"">
        <thead>
            <tr>
                <th>Descrição</th>
                <th>Quantidade</th>
                <th>Valor Unitário</th>
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Smartphone Samsung Galaxy</td>
                <td>1</td>
                <td>R$ 1.299,99</td>
                <td>R$ 1.299,99</td>
            </tr>
            <tr>
                <td>Película Protetora</td>
                <td>2</td>
                <td>R$ 25,00</td>
                <td>R$ 50,00</td>
            </tr>
            <tr>
                <td>Frete Expresso</td>
                <td>1</td>
                <td>R$ 20,00</td>
                <td>R$ 20,00</td>
            </tr>
        </tbody>
    </table>
    
    <div class=""total"">
        <p>Subtotal: R$ 1.369,99</p>
        <p>Desconto: R$ 50,00</p>
        <p><strong>Total: R$ 1.319,99</strong></p>
    </div>
    
    <div class=""footer"">
        <p>Esta é uma representação simplificada da Nota Fiscal Eletrônica.</p>
        <p>Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    </div>
</body>
</html>";
    }

    private static string GenerateWelcomeEmailHtml(string userName, string userEmail)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Bem-vindo ao E-Commerce!</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; }}
        .header {{ background-color: #3498db; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #e74c3c; color: white; text-decoration: none; border-radius: 4px; }}
        .footer {{ background-color: #ecf0f1; padding: 20px; text-align: center; font-size: 12px; color: #7f8c8d; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Bem-vindo ao E-Commerce!</h1>
        </div>
        
        <div class=""content"">
            <h2>Olá, {userName}!</h2>
            
            <p>Seja muito bem-vindo à nossa plataforma de e-commerce! Estamos muito felizes em tê-lo conosco.</p>
            
            <p>Sua conta foi criada com sucesso usando o e-mail <strong>{userEmail}</strong>.</p>
            
            <p>Agora você pode:</p>
            <ul>
                <li>Navegar por nosso catálogo de produtos</li>
                <li>Adicionar itens aos favoritos</li>
                <li>Fazer pedidos com entrega rápida</li>
                <li>Acompanhar seus pedidos em tempo real</li>
                <li>Participar de promoções exclusivas</li>
            </ul>
            
            <p>Para começar, que tal dar uma olhada em nossos produtos mais vendidos?</p>
            
            <p style=""text-align: center; margin: 30px 0;"">
                <a href=""#"" class=""button"">Explorar Produtos</a>
            </p>
            
            <p>Se você tiver alguma dúvida, nossa equipe de suporte está sempre disponível para ajudar.</p>
            
            <p>Mais uma vez, seja bem-vindo!</p>
            
            <p>Atenciosamente,<br>Equipe E-Commerce</p>
        </div>
        
        <div class=""footer"">
            <p>© 2024 E-Commerce Ltda. Todos os direitos reservados.</p>
            <p>Se você não deseja mais receber estes e-mails, <a href=""#"">clique aqui</a>.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GenerateDashboardWidgetHtml()
    {
        return @"
<div class=""dashboard-widget"" style=""border: 1px solid #ddd; border-radius: 8px; padding: 20px; margin: 16px; background: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
    <div class=""widget-header"" style=""display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 10px;"">
        <h3 style=""margin: 0; color: #333;"">Vendas do Mês</h3>
        <select style=""padding: 4px 8px; border: 1px solid #ddd; border-radius: 4px;"">
            <option>Janeiro 2024</option>
            <option>Dezembro 2023</option>
            <option>Novembro 2023</option>
        </select>
    </div>
    
    <div class=""widget-content"">
        <div class=""stats-grid"" style=""display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 20px;"">
            <div class=""stat-item"" style=""text-align: center; padding: 16px; background: #f8f9fa; border-radius: 4px;"">
                <div class=""stat-value"" style=""font-size: 24px; font-weight: bold; color: #2ecc71;"">R$ 45.670</div>
                <div class=""stat-label"" style=""color: #666; font-size: 14px;"">Receita Total</div>
            </div>
            <div class=""stat-item"" style=""text-align: center; padding: 16px; background: #f8f9fa; border-radius: 4px;"">
                <div class=""stat-value"" style=""font-size: 24px; font-weight: bold; color: #3498db;"">156</div>
                <div class=""stat-label"" style=""color: #666; font-size: 14px;"">Pedidos</div>
            </div>
            <div class=""stat-item"" style=""text-align: center; padding: 16px; background: #f8f9fa; border-radius: 4px;"">
                <div class=""stat-value"" style=""font-size: 24px; font-weight: bold; color: #e74c3c;"">R$ 293</div>
                <div class=""stat-label"" style=""color: #666; font-size: 14px;"">Ticket Médio</div>
            </div>
            <div class=""stat-item"" style=""text-align: center; padding: 16px; background: #f8f9fa; border-radius: 4px;"">
                <div class=""stat-value"" style=""font-size: 24px; font-weight: bold; color: #f39c12;"">89%</div>
                <div class=""stat-label"" style=""color: #666; font-size: 14px;"">Taxa Conversão</div>
            </div>
        </div>
        
        <div class=""chart-area"" style=""height: 200px; background: linear-gradient(45deg, #e3f2fd, #f3e5f5); border-radius: 4px; display: flex; align-items: center; justify-content: center; color: #666;"">
            <span>[Área do Gráfico - Vendas por Dia]</span>
        </div>
    </div>
    
    <div class=""widget-footer"" style=""margin-top: 20px; padding-top: 10px; border-top: 1px solid #eee; font-size: 12px; color: #999; text-align: right;"">
        Última atualização: há 5 minutos
    </div>
</div>";
    }

    private static string GenerateErrorPageHtml(int errorCode, string errorMessage)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Erro {errorCode} - {errorMessage}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .error-container {{
            text-align: center;
            background: white;
            padding: 60px 40px;
            border-radius: 20px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
            max-width: 500px;
        }}
        .error-code {{
            font-size: 120px;
            font-weight: bold;
            color: #e74c3c;
            margin: 0;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
        }}
        .error-message {{
            font-size: 24px;
            color: #2c3e50;
            margin: 20px 0;
            font-weight: 300;
        }}
        .error-description {{
            font-size: 16px;
            color: #7f8c8d;
            margin: 30px 0;
            line-height: 1.6;
        }}
        .error-actions {{
            margin-top: 40px;
        }}
        .btn {{
            display: inline-block;
            padding: 15px 30px;
            margin: 0 10px;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 500;
            transition: all 0.3s ease;
        }}
        .btn-primary {{
            background-color: #3498db;
            color: white;
        }}
        .btn-secondary {{
            background-color: #95a5a6;
            color: white;
        }}
        .btn:hover {{
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
        }}
        .error-icon {{
            font-size: 80px;
            margin-bottom: 20px;
        }}
    </style>
</head>
<body>
    <div class=""error-container"">
        <div class=""error-icon"">🚫</div>
        <div class=""error-code"">{errorCode}</div>
        <div class=""error-message"">{errorMessage}</div>
        <div class=""error-description"">
            {GetErrorDescription(errorCode)}
        </div>
        <div class=""error-actions"">
            <a href=""/"" class=""btn btn-primary"">Voltar ao Início</a>
            <a href=""/contato"" class=""btn btn-secondary"">Contato</a>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetErrorDescription(int errorCode)
    {
        return errorCode switch
        {
            404 => "A página que você está procurando não foi encontrada. Ela pode ter sido movida, removida ou você digitou o endereço incorretamente.",
            500 => "Ocorreu um erro interno no servidor. Nossa equipe foi notificada e está trabalhando para resolver o problema.",
            403 => "Você não tem permissão para acessar este recurso. Verifique se você está logado com as credenciais corretas.",
            401 => "Acesso não autorizado. Você precisa fazer login para acessar esta página.",
            _ => "Ocorreu um erro inesperado. Nossa equipe foi notificada e está investigando o problema."
        };
    }
}

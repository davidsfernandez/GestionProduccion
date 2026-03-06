/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Client.Resources
{
    public static class Portuguese
    {
        // Geral
        public const string AppName = "Gestão de Produção";
        public const string Save = "Salvar";
        public const string Create = "Criar";
        public const string Cancel = "Cancelar";
        public const string Edit = "Editar";
        public const string Delete = "Excluir";
        public const string Details = "Detalhes";
        public const string Loading = "Carregando...";
        public const string Success = "Sucesso!";
        public const string Error = "Erro!";
        public const string Actions = "Ações";
        public const string Search = "Buscar";
        public const string Filter = "Filtrar";
        public const string Print = "Imprimir";
        public const string Export = "Exportar";
        public const string Welcome = "Bem-vindo";
        public const string Profile = "Perfil";
        public const string Logout = "Sair";
        public const string ViewDetails = "Ver Detalhes";
        public const string ConfirmDelete = "Tem certeza que deseja excluir?";
        public const string ConfirmDeactivate = "Tem certeza que deseja desativar este usuário?";
        public const string Active = "Ativo";
        public const string Inactive = "Inativo";
        public const string BulkActions = "Ações em Massa";
        public const string NoRecordsFound = "Nenhum registro encontrado.";
        public const string Back = "Voltar";
        public const string Close = "Fechar";
        public const string Select = "Selecionar";
        public const string All = "Todos";
        public const string None = "Nenhum";
        public const string PleaseLogin = "Por favor, faça login";
        public const string Login = "Entrar";
        public const string Guest = "Convidado";
        public const string Selected = "Selecionado";
        public const string Pause = "Pausar";
        public const string Resume = "Retomar";
        public const string Stop = "Parar";
        public const string Quantity = "Quantidade";
        public const string Pending = "Pendente";
        public const string Cancelled = "Cancelado";
        public const string Deactivate = "Desativar";

        // Navegação
        public const string Nav_Dashboard = "Início";
        public const string Nav_Orders = "Ordens de Produção";
        public const string Nav_Products = "Catálogo de Produtos";
        public const string Nav_Users = "Usuários";
        public const string Nav_Teams = "Equipes de Costura";
        public const string Nav_Reports = "Relatórios";
        public const string Nav_Settings = "Configurações";
        public const string Nav_QA = "Qualidade (QA)";
        public const string Nav_DashboardTV = "Painel TV (Kanban)";
        public const string Nav_MyTasks = "Minhas Tarefas";
        public const string Nav_DelegateTasks = "Delegar Tarefas";
        public const string Nav_TvMode = "Modo TV";
        public const string Nav_Profile = "Meu Perfil";

        public const string Menu_Production = "PRODUÇÃO";
        public const string Menu_FactoryManagement = "GESTÃO DE FÁBRICA";
        public const string Menu_IntelligenceBI = "INTELIGÊNCIA & BI";
        public const string Menu_Settings = "CONFIGURAÇÕES";

        // Dashboard
        public const string Dash_Title = "Painel de Controle de Produção";
        public const string Dash_CompletionRate = "Taxa de Conclusão";
        public const string Dash_ActiveOrders = "Ordens Ativas";
        public const string Dash_CompletedToday = "Concluídas Hoje";
        public const string Dash_AvgLeadTime = "Tempo Médio de Ciclo";
        public const string Dash_Efficiency = "Eficiência Diária";
        public const string Dash_WeeklyVolume = "Volume Semanal (Últimos 7 Dias)";
        public const string Dash_ProductionStatus = "Status da Produção";
        public const string Dash_NormalOperation = "Operação Normal";
        public const string Dash_FactoryFloor = "No chão de fábrica";
        public const string Dash_OrdersByStage = "Ordens por Etapa";
        public const string Dash_AvgHours = "Média de horas";
        public const string Dash_RealTimeMonitoring = "Monitoramento em Tempo Real";
        public const string Dash_RealTime = "Tempo Real";
        public const string Dash_PerformanceUsers = "Desempenho por Operador";
        public const string Dash_Workload = "Carga de Trabalho por Usuário";
        public const string Dash_TvMode = "Modo TV";

        public const string Stage_Cutting = "Corte";
        public const string Stage_Sewing = "Costura";
        public const string Stage_Review = "Revisão";
        public const string Stage_Packaging = "Embalagem";
        public const string Stage_Completed = "Concluído";

        // Ordens de Produção (OP)
        public const string OP_Title = "Ordens de Produção";
        public const string OP_List = "Listagem de Ordens";
        public const string OP_Code = "Código";
        public const string OP_Product = "Produto";
        public const string OP_Quantity = "Quantidade";
        public const string OP_Stage = "Etapa Atual";
        public const string OP_Status = "Status";
        public const string OP_Priority = "Prioridade";
        public const string OP_AssignedTo = "Atribuído a";
        public const string OP_Unassigned = "Não atribuído";
        public const string OP_DueDate = "Data de Entrega";
        public const string OP_CreateOP = "Criar OP";
        public const string OP_NewOrder = "Nova Ordem";
        public const string OP_DailyPDF = "PDF Diário";
        public const string OP_Report = "Relatório PDF";
        public const string OP_NoOrdersFound = "Nenhuma ordem de produção encontrada.";
        public const string OP_NoOrdersMatch = "Nenhuma ordem corresponde aos critérios de busca.";
        public const string OP_FilterByStage = "Filtrar por Etapa";
        public const string OP_FilterByStatus = "Filtrar por Status";
        public const string OP_SortBy = "Ordenar por";
        public const string OP_SearchPlaceholder = "Produto ou Código...";
        public const string OP_ExportCSV = "Exportar CSV";
        public const string OP_ExportExcel = "Exportar Excel";
        public const string OP_BatchCount = "Lotes";

        public const string Status_Pending = "Pendente";
        public const string Status_InProgress = "Em Produção";
        public const string Status_InProduction = "Em Produção";
        public const string Status_Paused = "Pausado";
        public const string Status_Stopped = "Parado";
        public const string Status_Completed = "Finalizado";
        public const string Status_Finished = "Finalizado";
        public const string Status_Canceled = "Cancelado";

        // Criação / Detalhes de OP
        public const string OP_Create_Title = "Criar Nova Ordem de Produção";
        public const string OP_NewOrderTitle = "Nova Ordem de Produção";
        public const string OP_Details_Title = "Detalhes de Ordem de Produção";
        public const string OP_InfoDetails = "Informações Detalhadas";
        public const string OP_MainInfo = "Informações Principais";
        public const string OP_UniqueCode = "Código Único (Ex: OP-2024-001)";
        public const string OP_ProductDesc = "Descrição del Produto";
        public const string OP_EstimatedDelivery = "Previsão de Entrega";
        public const string OP_Delivery = "Entrega";
        public const string OP_CreationDate = "Data de Criação";
        public const string OP_History = "Histórico de Produção";
        public const string OP_AdvanceStage = "Avançar Etapa";
        public const string OP_Rework = "Retornar Etapa (Retrabalho)";
        public const string OP_ResumeProduction = "Retomar Produção";
        public const string OP_StopProduction = "Parar Produção";
        public const string OP_MarkCompleted = "Marcar como Concluído";
        public const string OP_AssignOperator = "Atribuir Operador";
        public const string OP_Note = "Observação / Nota";
        public const string OP_ReferencePhoto = "Foto de Referência";
        public const string OP_WorkflowDesc = "As ordens devem seguir o fluxo: Corte -> Costura -> Revisão -> Embalagem.";
        public const string OP_ConfirmRework = "Confirmar Retrabalho";
        public const string OP_OrderNotFound = "Ordem não encontrada.";
        public const string OP_ChangeAssignment = "Alterar atribuição...";
        public const string OP_Controls = "Controles de Produção";
        public const string OP_CurrentStageStatus = "Status da Etapa Atual";
        public const string OP_ReworkConfirmText = "Deseja realmente retornar esta ordem para a etapa anterior?";
        public const string OP_ReworkReason = "Motivo / Observação";
        public const string OP_ReworkRequired = "Obrigatório para retornar a una fase anterior.";
        public const string OP_QuickActions = "Ações Rápidas";
        public const string OP_ProductionMetrics = "Métricas de Produção";
        public const string OP_FinancialAnalysis = "Análise Financeira";
        public const string OP_EstimatedCost = "Custo Estimado";
        public const string OP_CostPerPiece = "Custo / Peça";
        public const string OP_ProfitMargin = "Margem de Lucro";
        public const string OP_PricePerPiece = "Preço Sugerido / Peça";
        public const string OP_TotalPrice = "Preço Total Estimado";
        public const string OP_ConfirmFinalize = "Confirmar Finalização";
        public const string OP_FinalizeWarning = "Tem certeza que deseja finalizar a produção desta ordem?";
        public const string OP_FinalizeEffects = "Esta ação irá encerrar o cronômetro, calcular custos e actualizar estoque.";
        public const string OP_LoadingSelectors = "Carregando seletores dinâmicos...";
        public const string OP_AutoGeneration = "Geração Automática";
        public const string OP_AutoGenerationDesc = "O código del lote (OP) será gerado automaticamente pelo sistema após salvar.";
        public const string OP_GeneratedOnSave = "Gerado ao Salvar";
        public const string OP_SelectProductFirst = "-- Selecione um Produto Primeiro --";
        public const string OP_SelectProduct = "Selecionar Produto";
        public const string OP_ProductReference = "Referência do Produto";
        public const string OP_ProductSearch = "Buscar Produto...";
        public const string OP_AssignTeamOptional = "Atribuir a uma equipe específica";
        public const string OP_AssignOperatorOptional = "Operador Específico (Opcional)";
        public const string OP_AssignOperatorOptionalPlaceholder = "Selecione o Operador (Opcional)";
        public const string OP_GeneralObservations = "Observações Gerais";
        public const string OP_CreateButton = "Gerar Ordem de Produção";
        public const string OP_SaveOrder = "Criar Ordem de Produção";
        public const string OP_OrderCreatedSuccess = "Ordem de Produção criada com sucesso!";
        public const string OP_OrderUpdatedSuccess = "Ordem de Produção atualizada com sucesso!";
        public const string OP_ProductionTip = "Dica de Produção";
        public const string OP_ProductionTipDesc = "Certifique-se de que a ficha técnica está anexada ao lote físico.";
        public const string OP_ErrValidationFieldsRed = "Por favor, corrija os errores destacados no formulário.";
        public const string OP_ErrLoadFormData = "Erro ao carregar dados del formulário.";
        public const string OP_ProfitPerOrder = "Lucro Líquido Esperado";
        public const string OP_FinancialStatus = "Status Financeiro da Ordem";
        public const string OP_RevenueTotal = "Receita Bruta Total";
        public const string OP_RealUnitCost = "Custo Real Unitário";
        public const string OP_UnitProfit = "Lucro por Peça";
        public const string OP_TimerActive = "CRONÔMETRO ATIVO";
        public const string OP_NotAssignedYet = "Esta ordem ainda não foi atribuída a un operador.";
        public const string OP_AdvanceInstructions = "Ao avançar, a etapa atual será concluída e a ordem passará para a próxima fase do fluxo.";
        public const string OP_ConfirmAdvance = "Confirmar Avanço";
        public const string OP_ReworkInstructions = "Esta ação retornará a ordem para a etapa anterior para correções.";
        public const string OP_ChangeStage = "Alterar Etapa";
        public const string OP_CurrentAssignment = "Atribuído Atualmente:";
        public const string OP_ReasonNote = "Observação/Motivo:";
        public const string OP_ChangeReasonPlaceholder = "Descreva o motivo da alteração...";
        public const string OP_ConfirmChange = "Confirmar Alteração";
        public const string OP_FinalizeOrder = "Finalizar Produção";
        public const string OP_ConfirmFinalizeOrder = "Tem certeza que deseja finalizar a produção da ordem";
        public const string OP_FinalizeActionWill = "Esta ação irá:";
        public const string OP_FinalizeStopTimer = "Encerrar o cronômetro de produção.";
        public const string OP_FinalizeCalculateCosts = "Calcular os custos finais com base no tempo gasto.";
        public const string OP_FinalizeMoveToHistory = "Mover a ordem para o histórico de concluídas.";
        public const string OP_PartialOutput = "Produção Parcial / Apontamento";
        public const string OP_PartialOutputDesc = "Registre a quantidade de peças que foram concluídas neste estágio hoje. O sistema avançará o estágio automaticamente quando todas as peças forem concluídas.";
        public const string OP_PiecesCompleted = "Peças Concluídas";
        public const string OP_AlreadyCompleted = "Já Concluído";
        public const string OP_Remaining = "Restante";
        public const string OP_SaveProduction = "Salvar Produção";
        public const string OP_ConfirmOutput = "Confirmar Registro";
        public const string OP_SuccessPartialOutput = "Produção registrada!";
        public const string OP_ErrorPartialOutput = "Erro ao registrar produção.";
        public const string OP_ErrLoadUsers = "Erro ao carregar usuários.";
        public const string OP_OrderSummary = "Resumo da Ordem";
        public const string OP_BackToList = "Voltar para Lista";
        public const string OP_AddSize = "Adicionar Tamanho";
        public const string OP_Size = "Tamanho";
        public const string OP_Qty = "Qtd";
        public const string OP_ClientOptional = "Cliente (Opcional)";
        public const string OP_SewingTeamOptional = "Equipe de Costura (Opcional)";
        public const string OP_Saving = "Salvando...";
        public const string OP_SizePlaceholder = "Tamanho...";
        public const string OP_ClientPlaceholder = "Nome do Cliente...";
        public const string OP_ExportPDF = "Exportar PDF";
        public const string ExportPDF = "Exportar PDF";
        public const string OP_TotalBatchCost = "Custo Total do Lote";
        public const string OP_Finalize = "Finalizar";
        public const string OP_By = "Por";
        public const string OP_Action = "Ação";
        public const string OP_OperatorAssigned = "Operador Atribuído";
        public const string OP_AssignNow = "Atribuir Agora";
        public const string OP_DetailedProgress = "Progresso Detalhado";
        public const string OP_InAndamento = "Em Andamento";
        public const string OP_SelectOperator = "Selecionar Operador";
        public const string OP_Select = "Selecionar";
        public const string OP_Assign = "Atribuir";
        public const string OP_NewStageLabel = "Nova Etapa";
        public const string OP_FinalizeCalcCosts = "Calcular custos finais.";
        public const string OP_FinalizeUpdateStock = "Atualizar estoque de produtos.";
        public const string OP_ConfirmAndFinalize = "Confirmar e Finalizar";
        public const string OP_PartialOutputStage = "Apontamento Parcial - Estágio";
        public const string OP_TotalOrder = "Total Ordem";
        public const string OP_QtyReadyNow = "Qtd Pronta Agora";
        public const string OP_ErrMinQty = "A quantidade deve ser maior que 0.";
        public const string OP_GeneratingPdf = "Gerando PDF...";
        public const string OP_ReportDefect = "Reportar Defeito / Retrabalho";
        public const string OP_AssignTask = "Atribuir Tarefa";
        public const string OP_SuccessGenerated = "Ordem de Produção gerada!";
        public const string OP_ErrSelectSizes = "Por favor, adicione pelo menos un tamanho e quantidade.";
        public const string OP_BackToListAlt = "Voltar para Listagem";

        // Tabela de Histórico
        public const string Hist_User = "Usuário";
        public const string Hist_Date = "Data/Hora";
        public const string Hist_Action = "Ação";
        public const string Hist_Details = "Detalhes";

        // Gestão de Usuários
        public const string User_Title = "Gestão de Usuários";
        public const string User_NewUser = "Novo Usuário";
        public const string User_User = "Usuário";
        public const string User_Name = "Nome Completo";
        public const string User_Email = "E-mail";
        public const string User_Role = "Perfil / Função";
        public const string User_Team = "Equipe / Setor";
        public const string User_Password = "Senha";
        public const string User_ConfirmPassword = "Confirmar Senha";
        public const string User_PublicId = "ID Público (UUID)";
        public const string User_Status = "Status da Conta";
        public const string User_IsAdmin = "Administrador do Sistema";
        public const string User_Permissions = "Permissões Específicas";
        public const string User_ChangePassword = "Alterar Senha";
        public const string User_CreateUser = "Cadastrar Usuário";
        public const string User_UpdateUser = "Atualizar Usuário";
        public const string User_Deactivate = "Desativar";
        public const string User_Activate = "Ativar";
        public const string User_SuccessCreate = "Usuário cadastrado com sucesso!";
        public const string User_SuccessUpdate = "Usuário atualizado com sucesso!";
        public const string User_ConfirmDeactivateTitle = "Desativar Usuário";
        public const string User_TeamSelection = "Vincular a uma Equipe (Opcional)";
        public const string User_Active = "Ativo";
        public const string User_Inactive = "Inativo";
        public const string User_FullNamePlaceholder = "Nome completo do usuário";
        public const string User_EmailPlaceholder = "Email institucional";
        public const string User_PassHint = "Mínimo 6 caracteres";

        public const string Role_Admin = "Administrador";
        public const string Role_Manager = "Gerente / Supervisor";
        public const string Role_Operator = "Operador de Produção";
        public const string Role_QA = "Analista de Qualidade";
        public const string Role_Leader = "Líder de Equipe";
        public const string Role_Office = "Escritório / Administrativo";
        public const string Role_Workshop = "Oficina / Produção";

        // Qualidade (QA)
        public const string QA_Title = "Controle de Qualidade";
        public const string QA_Inspection = "Inspeção de Lote";
        public const string QA_Status = "Status da Inspeção";
        public const string QA_DefectsFound = "Defeitos Encontrados";
        public const string QA_Approved = "Aprovado";
        public const string QA_Rejected = "Reprovado";
        public const string QA_Notes = "Observações de Qualidade";
        public const string QA_Summary = "Resumo de Qualidade";
        public const string QA_Reason = "Motivo do Defeito";
        public const string QA_Photo = "Foto do Problema";
        public const string QA_NoDefects = "Nenhum defeito reportado.";

        // Produtos / Catálogo
        public const string Prod_Title = "Catálogo de Produtos";
        public const string Prod_NewProduct = "Novo Produto";
        public const string Prod_Reference = "Referência / SKU";
        public const string Prod_Name = "Nome do Produto";
        public const string Prod_Category = "Categoria";
        public const string Prod_BasePrice = "Preço de Venda Base";
        public const string Prod_EstimatedCost = "Custo de Produção Estimado";
        public const string Prod_Description = "Descrição Técnica";
        public const string Prod_Image = "Foto do Produto";
        public const string Prod_Stock = "Estoque";
        public const string Prod_MinStock = "Estoque Mínimo";
        public const string Prod_Sizes = "Grade de Tamanhos";
        public const string Prod_Colors = "Cores Disponíveis";
        public const string Prod_SuccessCreate = "Produto cadastrado com sucesso!";
        public const string Prod_SuccessUpdate = "Produto atualizado com sucesso!";
        public const string Prod_ConfirmDeleteTitle = "Excluir Produto";

        public const string Cat_Title = "Catálogo";
        public const string Cat_NewProduct = "Novo Produto";
        public const string Cat_EditProduct = "Editar Produto";
        public const string Cat_SearchHint = "Buscar por nome ou referência...";
        public const string Cat_FabricType = "Tipo de Tecido";
        public const string Cat_AvgTime = "Tempo Médio";
        public const string Cat_AvgTimeMinutes = "Tempo Médio (Minutos)";
        public const string Cat_Price = "Preço";
        public const string Cat_EstPrice = "Preço Est.";
        public const string Cat_NoProducts = "Nenhum produto cadastrado no catálogo.";
        public const string Cat_InternalCode = "Código Interno";
        public const string Cat_MainSku = "Referência (SKU)";
        public const string Cat_SaveProduct = "Salvar Produto";
        public const string Cat_ErrValidationFields = "Erro de validação no formulário.";
        public const string Cat_ErrLoad = "Erro ao carregar catálogo.";
        public const string Cat_SuccessCreated = "Produto criado!";
        public const string Cat_SuccessUpdated = "Produto atualizado!";
        public const string Cat_ErrDuplicateCode = "Este código ou referência já existe.";
        public const string Cat_ErrServerInaccessible = "Servidor inacessível.";
        public const string Cat_ConfirmDelete = "Excluir Produto?";
        public const string Cat_SuccessDeleted = "Produto excluído!";
        public const string Cat_ErrDeleteActiveOrders = "Não é possível excluir produtos com ordens ativas.";
        public const string Cat_ConflictData = "Conflito de dados.";
        public const string Cat_Sizes = "Tamanhos";
        public const string Cat_ErrValidation = "Erro de validação.";
        public const string Cat_NoLinkedProduct = "Sem produto vinculado";

        // Equipes (Teams)
        public const string Team_Title = "Equipes de Costura";
        public const string Team_NewTeam = "Nova Equipe";
        public const string Team_Name = "Nome da Equipe";
        public const string Team_Supervisor = "Supervisor Responsável";
        public const string Team_MembersCount = "Integrantes";
        public const string Team_Efficiency = "Eficiência da Equipe";
        public const string Team_Capacity = "Capacidade Diária (Peças)";
        public const string Team_CurrentTask = "Tarefa Atual";
        public const string Team_Members = "Membros da Equipe";
        public const string Team_AddMember = "Adicionar Membro";
        public const string Team_SuccessCreate = "Equipe criada com sucesso!";
        public const string Team_SuccessUpdate = "Equipe atualizada com sucesso!";
        public const string Team_ReassignmentBlock = "Esta equipe possui ordens ativas e não pode ser desativada agora.";
        public const string Team_ConfirmDelete = "Tem certeza que deseja excluir esta equipe?";
        public const string Team_SuccessDeleted = "Equipe excluída com sucesso!";
        public const string Team_ErrNoOtherTeams = "Não há outras equipes disponíveis.";
        public const string Team_ErrSave = "Erro ao salvar equipe.";
        public const string Team_ErrNoEligibleUsers = "Não há operadores disponíveis para equipes.";
        public const string Team_AddTeam = "Criar Equipe";
        public const string Team_NoTeamsFound = "Nenhuma equipe cadastrada.";
        public const string Team_EditTeam = "Editar Equipe";
        public const string Team_SelectMemberHint = "Selecione os membros desta equipe";
        public const string Team_ManageMembers = "Gerenciar Membros";
        public const string Team_ActiveTeam = "Equipe Ativa";
        public const string Team_SaveTeam = "Salvar Equipe";
        public const string Team_ErrLoadData = "Erro ao carregar equipes.";
        public const string Team_SuccessMembersUpdated = "Membros atualizados!";
        public const string Team_SuccessSaved = "Equipe salva!";
        public const string Team_ErrBusiness = "Erro de regra de negócio.";

        // Configurações (Settings)
        public const string Set_Title = "Configurações do Sistema";
        public const string Set_General = "Geral";
        public const string Set_Financial = "Financeiro & Custos";
        public const string Set_Company = "Dados da Empresa";
        public const string Set_CompanyName = "Nome da Fábrica";
        public const string Set_Theme = "Tema Visual";
        public const string Set_Language = "Idioma do Sistema";
        public const string Set_TaxRate = "Alíquota de Impostos (%)";
        public const string Set_OperationalCostHour = "Custo Operacional / Hora";
        public const string Set_ProfitMarginDefault = "Margem de Lucro Padrão (%)";
        public const string Set_Backup = "Backup de Dados";
        public const string Set_SuccessUpdate = "Configurações salvas!";
        public const string Set_LogoUpload = "Alterar Logotipo";
        public const string Set_FinancialParams = "Parâmetros Financeiros";
        public const string Set_Branding = "Personalização (Branding)";
        public const string Set_Important = "Atenção: Estas alterações impactam os cálculos de custo real de todas as OPs futuras.";
        public const string Set_ReportHint = "Relatórios e PDFs exibirão o nome e logo configurados abaixo.";
        public const string Set_LogoQualityHint = "Use imagens PNG o JPG quadradas para melhor qualidade.";
        public const string Set_ErrLoad = "Erro ao carregar configurações.";
        public const string Set_ErrFileTooLarge = "Arquivo muito grande (Máx 1MB).";
        public const string Set_ErrReadImage = "Erro ao ler imagem.";
        public const string Set_SuccessSave = "Configurações salvas com sucesso!";
        public const string Set_ErrSave = "Erro ao salvar configurações.";
        public const string Set_CompanyInfo = "Informações Jurídicas";
        public const string Set_CompanyTaxId = "CNPJ / Identificação Fiscal";
        public const string Set_VisualIdentity = "Identidade Visual";
        public const string Set_RemoveLogo = "Remover Logo";
        public const string Set_NoLogo = "Sem Logotipo";
        public const string Set_UploadLogoHint = "Upload de Logotipo (Recomendado 256x256)";
        public const string Set_ThemeCustomization = "Personalização de Tema";
        public const string Set_ThemeChoice = "Escolha o esquema de cores principal";
        public const string Set_DailyFixedCost = "Custo Fixo Diário (Total)";
        public const string Set_DailyFixedCostHint = "Soma de aluguel, luz, etc.";
        public const string Set_HourlyCost = "Valor Hora/Homem Médio";
        public const string Set_HourlyCostHint = "Média salarial por hora.";
        public const string Set_TvMode = "Configuração do Modo TV";
        public const string Set_TvAnnouncement = "Aviso / Mural de Recados";
        public const string Set_TvAnnouncementPlaceholder = "Ex: Reunião geral às 15h...";
        public const string Set_TvAnnouncementHint = "Este texto rolará na parte inferior do painel de TV.";
        public const string Set_SaveSettings = "Salvar Todas as Configurações";

        // Relatórios
        public const string Rep_Title = "Relatórios de Produção";
        public const string Rep_ProductionByPeriod = "Produção por Período";
        public const string Rep_EfficiencyOperator = "Eficiência por Operador";
        public const string Rep_CostAnalysis = "Análise de Custos e Lucro";
        public const string Rep_InventoryStatus = "Posição de Estoque";
        public const string Rep_StartDate = "Data Início";
        public const string Rep_EndDate = "Data Fim";
        public const string Rep_Generate = "Gerar Relatório";
        public const string Rep_ExportExcel = "Exportar Excel";
        public const string Rep_ExportPDF = "Exportar PDF";

        // Tarefas (Tasks)
        public const string Task_DelegateAdmin = "Delegar Tarefa Administrativa";
        public const string Task_NewTask = "Nova Tarefa";
        public const string Task_TitleLabel = "Título da Tarefa";
        public const string Task_Description = "Descrição / Instruções";
        public const string Task_DescriptionPlaceholder = "O que deve ser feito?";
        public const string Task_Responsible = "Responsável";
        public const string Task_SelectUser = "Selecione o usuário...";
        public const string Task_Deadline = "Prazo de Conclusão";
        public const string Task_DeadlineShort = "Prazo";
        public const string Task_RecentDelegated = "Tarefas Delegadas Recentemente";
        public const string Task_Task = "Tarefa";
        public const string Task_ErrLoadData = "Erro ao carregar tarefas.";
        public const string Task_ErrSelectResponsible = "Selecione um responsável.";
        public const string Task_ErrCreate = "Erro ao criar tarefa.";
        public const string Task_InProgress = "Em Andamento";
        public const string Task_Completed = "Concluída";

        // Erros e Mensagens
        public const string Err_Required = "Campo obrigatório.";
        public const string Err_InvalidEmail = "E-mail inválido.";
        public const string Err_PasswordTooShort = "A senha deve ter pelo menos 6 caracteres.";
        public const string Err_Generic = "Ocorreu um erro inesperado.";
        public const string Err_Unauthorized = "Você não tem permissão para acessar esta área.";
        public const string Err_NotFound = "Recurso não encontrado.";
        public const string Err_Connection = "Erro de conexão com o servidor.";
        public const string Msg_Error = "Ocorreu um erro.";
        public const string Msg_OrderUpdated = "Ordem atualizada com sucesso!";
        public const string Msg_UserUpdated = "Usuário atualizado!";
        public const string Msg_UserCreated = "Usuário criado!";
        public const string Msg_SystemInitializing = "Inicializando sistema...";
        public const string Msg_SystemChecking = "Verificando sistema...";
        public const string Msg_LoginFailed = "Falha no login. Verifique suas credenciais.";
        public const string Msg_PassChanged = "Senha alterada com sucesso!";
        public const string Msg_TaskAssigned = "Tarefa atribuída!";
        public const string Toast_Close = "Fechar";
        public const string Toast_SystemNotice = "Aviso do Sistema";

        // Perfil
        public const string Prof_Title = "Meu Perfil";
        public const string Prof_ChangePass = "Alterar Senha de Acesso";
        public const string Prof_CurrentPass = "Senha Atual";
        public const string Prof_NewPass = "Nova Senha";
        public const string Prof_ConfirmPass = "Confirmar Nova Senha";

        // Catálogo (Legacy/Complementary)
        public const string Product = "Produto";
    }
}

# Общая информация
Данное расширение для Visual Studio 2022 позволяет отслеживать изменения зависимостей между проектами и сопоставлять эти зависимости с правилами, заданными в специальном конфиг-файле. В случае несоответствия изменений связей заданным настройкам, расширение выдаёт предупреждение о некорректных связях и не даёт сборке завершиться.

Целью данного приложения является контроль связей в решениях и обеспечение их соответствия стандартному виду, принятому в компании. Реализуется в рамках администрирования сборки.

# Ссылки на прочие связанные документы
[User_Guide](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%BE%D0%B1%D1%89%D0%B0%D1%8F-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F)</br>

[Starter Guide](https://github.com/Roman194/RefDepGuard/blob/master/STARTER_GUIDE.md)

# Возможности расширения
_Подробнее ознакомиться с возможностями расширения можно в USER_GUIDE!_

## Оглавление
[0. Способы взаимодействия с расширением](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#0-%D1%81%D0%BF%D0%BE%D1%81%D0%BE%D0%B1%D1%8B-%D0%B2%D0%B7%D0%B0%D0%B8%D0%BC%D0%BE%D0%B4%D0%B5%D0%B9%D1%81%D1%82%D0%B2%D0%B8%D1%8F-%D1%81-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D0%B5%D0%BC) </br>
</br>
[1. Активация расширения](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#1-%D0%B0%D0%BA%D1%82%D0%B8%D0%B2%D0%B0%D1%86%D0%B8%D1%8F-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F)</br>
</br>
[2. Работа расширения в фоновом режиме](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#2-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%B2-%D1%84%D0%BE%D0%BD%D0%BE%D0%B2%D0%BE%D0%BC-%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B5)</br>
- [а. Возможные ошибки и предупреждения расширения](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B0-%D0%B2%D0%BE%D0%B7%D0%BC%D0%BE%D0%B6%D0%BD%D1%8B%D0%B5-%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8-%D0%B8-%D0%BF%D1%80%D0%B5%D0%B4%D1%83%D0%BF%D1%80%D0%B5%D0%B6%D0%B4%D0%B5%D0%BD%D0%B8%D1%8F-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F)</br>

[3. Работа с конфигурационными файлами](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#3-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0-%D1%81-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D1%8B%D0%BC%D0%B8-%D1%84%D0%B0%D0%B9%D0%BB%D0%B0%D0%BC%D0%B8)</br>
- [а. Шаблон файла "global_config_guard.rdg"](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B0-%D1%88%D0%B0%D0%B1%D0%BB%D0%BE%D0%BD-%D1%84%D0%B0%D0%B9%D0%BB%D0%B0-global_config_guardrdg-%D0%B8-%D0%BE%D1%81%D0%BE%D0%B1%D0%B5%D0%BD%D0%BD%D0%BE%D1%81%D1%82%D0%B8-%D0%BF%D0%B0%D1%80%D0%B0%D0%BC%D0%B5%D1%82%D1%80%D0%B0-framework_max_version)</br>
- [б. Шаблон файла "{название Solution}_config_guard.rdg"](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B1-%D1%88%D0%B0%D0%B1%D0%BB%D0%BE%D0%BD-%D1%84%D0%B0%D0%B9%D0%BB%D0%B0-%D0%BD%D0%B0%D0%B7%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5-solution_config_guardrdg)</br>
- [в. Различные уровни правил и их приоритизация](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B2-%D1%80%D0%B0%D0%B7%D0%BB%D0%B8%D1%87%D0%BD%D1%8B%D0%B5-%D1%83%D1%80%D0%BE%D0%B2%D0%BD%D0%B8-%D0%BF%D1%80%D0%B0%D0%B2%D0%B8%D0%BB-%D0%B8-%D0%B8%D1%85-%D0%BF%D1%80%D0%B8%D0%BE%D1%80%D0%B8%D1%82%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D1%8F)</br>
- [г. Способы создания файлов конфигурации](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B3-%D1%81%D0%BF%D0%BE%D1%81%D0%BE%D0%B1%D1%8B-%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D1%8F-%D1%84%D0%B0%D0%B9%D0%BB%D0%BE%D0%B2-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D0%B8)</br>

[4. Вывод всех текущих референсов проекта](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#4-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D0%B2%D1%81%D0%B5%D1%85-%D1%82%D0%B5%D0%BA%D1%83%D1%89%D0%B8%D1%85-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0)</br>
- [а. Вывод прямых референсов](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B0-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D0%BF%D1%80%D1%8F%D0%BC%D1%8B%D1%85-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2)</br>
- [б. Вывод транзитивных референсов](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B1-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D1%82%D1%80%D0%B0%D0%BD%D0%B7%D0%B8%D1%82%D0%B8%D0%B2%D0%BD%D1%8B%D1%85-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2)</br>
[5. Вывод изменений референсов с момента их последней фиксации](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#5-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D0%B8%D0%B7%D0%BC%D0%B5%D0%BD%D0%B5%D0%BD%D0%B8%D0%B9-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2-%D1%81-%D0%BC%D0%BE%D0%BC%D0%B5%D0%BD%D1%82%D0%B0-%D0%B8%D1%85-%D0%BF%D0%BE%D1%81%D0%BB%D0%B5%D0%B4%D0%BD%D0%B5%D0%B9-%D1%84%D0%B8%D0%BA%D1%81%D0%B0%D1%86%D0%B8%D0%B8)</br>

[6. Принудительная фиксация референсов](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#6-%D0%BF%D1%80%D0%B8%D0%BD%D1%83%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C%D0%BD%D0%B0%D1%8F-%D1%84%D0%B8%D0%BA%D1%81%D0%B0%D1%86%D0%B8%D1%8F-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2)</br>

[7. Табличный экспорт состояния проекта](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#7-%D1%82%D0%B0%D0%B1%D0%BB%D0%B8%D1%87%D0%BD%D1%8B%D0%B9-%D1%8D%D0%BA%D1%81%D0%BF%D0%BE%D1%80%D1%82-%D1%81%D0%BE%D1%81%D1%82%D0%BE%D1%8F%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0)</br>
- [а. Страница выборки по проектам](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B0-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D0%B2%D1%8B%D0%B1%D0%BE%D1%80%D0%BA%D0%B8-%D0%BF%D0%BE-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0%D0%BC)</br>
- [б. Страница выборки по референсам](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B1-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D0%B2%D1%8B%D0%B1%D0%BE%D1%80%D0%BA%D0%B8-%D0%BF%D0%BE-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%B0%D0%BC)</br>
- [в. Страница текущих ошибок RefDepGuard](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B2-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D1%82%D0%B5%D0%BA%D1%83%D1%89%D0%B8%D1%85-%D0%BE%D1%88%D0%B8%D0%B1%D0%BE%D0%BA-refdepguard)</br>
- [г. Страница текущих предупреждений RefDepGuard](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#%D0%B3-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D1%82%D0%B5%D0%BA%D1%83%D1%89%D0%B8%D1%85-%D0%BF%D1%80%D0%B5%D0%B4%D1%83%D0%BF%D1%80%D0%B5%D0%B6%D0%B4%D0%B5%D0%BD%D0%B8%D0%B9-refdepguard)</br>

[8. Графический экспорт состояния проекта](https://github.com/Roman194/RefDepGuard/tree/master?tab=readme-ov-file#8-%D0%B3%D1%80%D0%B0%D1%84%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B9-%D1%8D%D0%BA%D1%81%D0%BF%D0%BE%D1%80%D1%82-%D1%81%D0%BE%D1%81%D1%82%D0%BE%D1%8F%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0)

## 0. Способы взаимодействия с расширением
После установки расширение поддерживает 2 способа взаимодействия:
- Через меню расширений

Для этого на верхней панели инструментов выберите раздел "Расширения"</br>

<img width="1074" height="122" alt="Items" src="https://github.com/user-attachments/assets/72cdd957-2403-4d2b-ad31-665e6eb29143" />
    
Среди расширений выберите расширение "RefDepGuardian".
Весь функционал расширения рассмотрен в основных пунктах этого расширения и представлен на рисунке ниже:</br>
<img width="732" height="214" alt="image" src="https://github.com/user-attachments/assets/fa78db42-4080-4a2c-bec0-f485e900d50d" />

Для выполнения требуемой функциональности необходимо нажать на соответствующую кнопку (см. рис выше)

- С помощью "горячих" клавиш

Как можно заметить по рисунку выше, весь функционал расширения продублирован "горячими клавишами".</br>
То есть для запуска необходимой функции достаточно нажать соответствующую комбинацию клавиш при загруженном в Visual Studio, для которого предполагается выполнить действие.</br>

## 1. Активация расширения
При первой загрузке решения расширение выведет сообщение, где позволит выбрать, нужно ли использовать расширение в данном решении или нет. В случае согласия на использование, запускается работа расширения в фоновом режиме и через "хоткеи" и основное меню расширения становится доступен весь его функционал.</br>

<img width="472" height="223" alt="image" src="https://github.com/user-attachments/assets/f44bdbb9-49c5-4016-aecb-dff75e38ff62" /></br>

В противном случае расширение остаётся неактивированным, но пользователь может активировать расширение с помощью специальной кнопки, выводящейся вместо основного меню расширения:</br>

<img width="647" height="140" alt="image" src="https://github.com/user-attachments/assets/bc290738-e2f2-44da-ac10-7258baabdf93" />

_!Важно: на текущий момент расширение нельзя деактивировать, поэтому решение об его активации на конкретном решении должно быть взвешенным!_ 

## 2. Работа расширения в фоновом режиме
Расширение функционирует не только по команде пользователя, но и в фоновом режиме.
В фоновом режиме оно производит фиксацию текущего состояния связей между проектами и версий TargetFramework проекта, а также проверку соответствия этих параметров ограничениям, указанным в **файле конфигурации**. 
Расширение производит работу в фоновом режиме после стартовой загрузки/открытия решения и во время сборки проекта (решения). В случае обнаружения несоответствия текущих параметров проектов и связей между ними, расширение выводит в "список ошибок" VS22 соответствующие ошибки и предупреждения расширения.
В случае обнаружения ошибок во время сборки, расширение **отменяет сборку** и не даёт ей успешно завершиться до тех пор, пока не будут устранены все найденные расширением ошибки.

<img width="1280" height="285" alt="image" src="https://github.com/user-attachments/assets/94efbe55-8a9c-49cc-ba82-638bac91d6bd" />

Список ошибок (Errors) и предупреждений (Warnings), которые может выдавать расширение, приведён ниже.
В случае, если никакие проблемы не были обнаружены, при завершении проверки соблюдения правил расширением выводиться соответствующее сообщение:</br>

<img width="730" height="151" alt="image" src="https://github.com/user-attachments/assets/61147c52-d160-42cd-bb98-83c088c7aac3" />


Отдельно выделен случай, когда расширению не удалось обнаружить в решении ни одни референс между проектами. Считается потенциально ошибочным действием (учитывая то, что предполагается использовать расширение в решениях, где представлено множество референсов между проектами), поэтому помечается как ошибка. При обнаружении отсутствия референсов проверка на соблюдение правил конфигурационного фала **не производится** и данное предупреждение выводится в панель ошибок как единственное уведомление от расширения:</br>

<img width="1163" height="128" alt="image" src="https://github.com/user-attachments/assets/6bd53807-42a6-43a1-a2ca-f6e5a7a93b25" />

_!Важно: причиной отсутствия обнаружения референсов расширением после загрузки solution может стать тот факт, что на момент их проверки solution всё ещё не был до конца загружен. При возникновении подобного предупреждения после загрузки попробуйте выполнить их принудительную фиксацию!_ 

_Расширение **будет считаться загруженным**, когда либо будет выведен список обнаруженных проблем, либо предупреждение об отсутствии референсов в solution, либо сообщение о том, что расширение не обнаружило никаких проблем!_ 

### а) Возможные ошибки и предупреждения расширения

***Ошибки*:**

**Reference error** - Обнаружен референс, создание которого запрещено в рамках конфигурационного файла или не обнаружен референс, который должен обязательно существовать по требованию из конфиг-файла.</br>
</br>
**Match error** - В конфиг-файле на одном уровне обнаружено противоречие в правиле референсов: референс одновременно заявлен как обязательный и недопустимый.</br>
</br>
**Null property error** - В конфиг-файле не было обнаружено одно или несколько из требуемых по шаблону конфиг-файла свойств.</br>
</br>
**Framework version comparability error** - Когда текущая версия TargetFramework проекта превосходит максимально допустимую версию для этого проекта (ограничение в рамках параметра "max_framework_version" конфиг-файла).</br>
</br>
**framework_max_version deviant value error** - В значении параметра "max_framework_version" обнаружено некорректное значение.</br>
</br>
**framework_max_version template illegal usage error** - В значении параметра "max_framework_version" обнаружено некорректное использование шаблона задания различных ограничений для различных типов проектов.</br>

***Предупреждения*:**

**warning** - Обнаружено некорректное значение свойства TargetFramework (по мнению расширения) в .csproj проекта. **При корректной работе расширения возникать не должно**.</br>
</br>
**Reference Match Warning** - Дублирование правила референсов или их противоречие на различных уровнях.</br>
</br>
**framework_max_version conflict warning** - В конфиг-файле значение более глобального параметра "framework_max_version" имеет меньшее значение, чем более локальное; или когда на одном уровне значение супертипа all больше, чем значение одного из типов.</br>
</br>
**framework_max_version reference conflict warning** - Обнаружен референс между проектами, среди которых ссылающийся проект имеет меньшее значение "framework_max_version", чем тот, на кого он ссылается.
Также к данному типу предупреждений относятся проблемы, когда текущая версия рефа типа "netstandard" несовместима с текущей версией проекта (согласно его TFM).</br>
</br>
**Project not found warning** - В связи между проектами обнаружено значение, не соответствующее ни одному из названий существующих проектов решения.</br>
</br>
**Project Match Warning** - В решении обнаружен проект, которого нет в конфиг-файле этого Solution или когда в конфиг-файле проект есть, а в самом решении - нет.</br>
</br>
**framework_max_version TFM not found warning** - В шаблоне задания различных ограничений для различных типов проектов задан TFM, не соответствующий ни одному из существующих согласно [официальной документации](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)</br>
</br>
**framework_max_version deviant value warning** - В значении параметра "max_framework_version" обнаружено несоответствие формату "x.0", где x - любое число.</br>
</br>
**Transit references warning** - Выводится, если в решении обнаружены транзитивные связи между проектами и в конфиг-файле проставлено разрешение на вывод транзитивных связей на всех 3-х уровнях.</br>

## 3. Работа с конфигурационными файлами
Конфигурационные файлы представляют собой 2 файла с расширением .rdg, в которых фиксируются все правила, которым должны соответствовать проекты и референсы между ними в рассматриваемом Solution.
Для корректного прочтения расширением оба файла должны находиться в корневом каталоге Solution (там же, где лежит соответствующий решению .sln файл).
Их структура представляет собой JSON-файлы, в которых рядом с параметрами записаны значения, по которым эти параметры будут сверяться с фактическим положением дел в загруженном Solution.

Названия этих файлов:
````JSON
"global_config_guard.rdg" и "{название Solution}_config_guard.rdg"
````
**global_config_guard.rdg** представляет собой файл глобальных конфигураций: в нём задаются правила, общие для нескольких Solution. </br>
</br>
**{название Solution}_config_guard.rdg** представляет собой файл конфигураций конкретного Solution. В нём указаны все правила, относящиеся к Solution, которое заявлено в названии файла.

### а) Шаблон файла "global_config_guard.rdg":
````JSON
{
	"name":"Global",
	"framework_max_version":"-",
	"report_on_transit_references": false,
	"global_required_references":[
              "Mir.Controller.Model"
        ],
	"global_unacceptable_references":[
               "Mir.Controller.Cfg.Meters.CntCfg"
        ]
}
````

### б) Шаблон файла "{название Solution}_config_guard.rdg"
````JSON
{
  "name": "WinFormApp",
  "framework_max_version": "netstandard: 2.0.0; net: 8.0",
  "report_on_transit_references": false,
  "solution_required_references":[
    "Mir.Controller.Main",
    "Mir.Controller.Tests.Cfg"
  ],
  "solution_unacceptable_references":[

  ],
  "projects": {
    "Mir.Controller.Model":{
      "framework_max_version": "3.7.0",
      "report_on_transit_references": false,
      "consider_global_and_solution_references": {
          "required": true,
          "unacceptable": true
      }
      "required_references":[
         "Mir.Controller.Tests",
         "Mir.Controller.Project1"
      ],
      "unacceptable_references":[
        "Mir.Controller.WinProject"
      ]
    },
    "Mir.Controller.ARP56":{
      "framework_max_version": "4.7.2",
      "report_on_transit_references": false,
      "consider_global_and_solution_references": {
          "required": true,
          "unnacceptable": true
      }
      "required_references":[
         "Mir.Controller.Tests",
         "Mir.Controller.Project1"
      ],
      "unacceptable_references":[
         "Mir.Controller.WinProject"
      ]
    }
  }
}

````

### в) Различные уровни правил и их приоритизация
Параметры "framework_max_version", "required_references", "unacceptable_references" и им подобные задаются на различных уровнях правил.
Всего в расширении 3 уровня правил:
- **Global** - правила глобального уровня (относятся ко всем проектам текущей папки)
- **Solution** - правила уровня текущего решения (относятся ко всем проектам текущего Solution)
- **Project** - правила текущего проекта (относятся к конкретному проекту)

**Приоритизация правил**:
Правило уровня Project > Правило уровня Solution > Правило уровня Global

_! Исключение: в параметре "report_on_transit_references" используется обратная логика приоритезации!_

Логика приоритизации правил срабатывает в случае, если обнаружено противоречие между правилами. Так расширение решает, какое правило важнее и должно быть использовано. Пользователю же выводится соответствующее предупреждение об обнаружении противоречия: **framework_max_version conflict warning** или **Match warning** соответственно.

В случае, если **противоречащие правила имеют один уровень**, оба правила не учитываются и выводится соответствующая предупреждение/ошибка: **framework_max_version conflict warning** или **Match error** соответственно.

### г) Способы создания файлов конфигурации
Вы можете создать файлы конфигурации одним из следующих способов:
- Самостоятельно по указаниям и шаблонам из User Guide
- Воспользовавшись автогенерацией шаблона расширением при необнаружении файла в корневой директории Solution (рекомендуется)

## 4. Вывод всех текущих референсов проекта
### а) Вывод прямых референсов
При нажатии в меню на кнопку "Показать все прямые рефы" или при нажатии комбинации клавиш Alt + R:</br>

<img width="355" height="809" alt="image" src="https://github.com/user-attachments/assets/6bc9c2bd-b64c-4ffc-b25f-f9d42741eac7" />

### б) Вывод транзитивных референсов

При нажатии в меню на кнопку "Показать все транзитивные рефы" или при нажатии комбинации клавиш Alt + T:</br>

<img width="305" height="646" alt="image" src="https://github.com/user-attachments/assets/1e633fc7-182f-46fa-848a-3395a56c9ee5" />

## 5. Вывод изменений референсов с момента их последней фиксации
При нажатии в меню на кнопку "Вывести изменения в рефах" или при нажатии комбинации клавиш Alt + E:</br>

<img width="436" height="286" alt="image" src="https://github.com/user-attachments/assets/e45382d0-6e25-493d-a1fb-f2de818debe8" />

## 6. Принудительная фиксация референсов
При нажатии в меню на кнопку "Зафиксировать текущую версию решения" или при нажатии комбинации клавиш Alt + C производится фиксация текущих референсов между проектами, текущих TargetFramework версий проекта и проверка соответствия этих текущих параметров правилам, загруженным из файлов конфигурации.
По завершении выполнения проверки выводится либо список найденных расширением проблем, либо предупреждение об отсутствии референсов между проектами, либо сообщение о том, что проблемы в решении не были обнаружены.
Также выводится сообщение (MessageBox) с результатом успешности выполнения фиксации:</br>

<img width="321" height="172" alt="image" src="https://github.com/user-attachments/assets/74144246-02f3-42b4-88c9-15cd5399dae5" /></br>

<img width="327" height="175" alt="image" src="https://github.com/user-attachments/assets/240dd505-fc10-4a73-9d43-7a2de51efcc1" /></br>

<img width="450" height="173" alt="image" src="https://github.com/user-attachments/assets/6ea56a9e-ded4-4c5f-b2f3-d018eca2fc38" />

## 7. Табличный экспорт состояния проекта
При нажатии в меню на кнопку "Экспорт в XLSX" или при нажатии комбинации клавиш Alt + X производится формирование табличного отчёта в формате .xlsx файла.

По результатам экспорта пользователю будет выведен MessageBox с предложением открыть папку с сохранённым экспортом.</br>

<img width="434" height="160" alt="image" src="https://github.com/user-attachments/assets/2904ac6b-c00d-40c6-9407-22b1761d3562" />

Файл будет сохранён по следующему пути и будет иметь следующее название:
````JSON
{название_solution}/reports/table_type/{DD.MM.YYYY-HH.MM.SS}/{Название solution}_references_report.xlsx
````
Табличный отчёт состоит из трёх страниц:
- Страница выборки по проектам
- Выборки по референсам
- Текущих ошибок RefDepGuard
- Текущих предупреждений RefDepGuard

### а) Страница выборки по проектам

<img width="975" height="263" alt="image" src="https://github.com/user-attachments/assets/60cfba88-3f83-4c15-a6b9-1cbb6bcac6b2" />

### б) Страница выборки по референсам

<img width="528" height="491" alt="image" src="https://github.com/user-attachments/assets/1c61168e-2c78-44ab-92e7-80429da551ec" />

### в) Страница текущих ошибок RefDepGuard

<img width="999" height="214" alt="image" src="https://github.com/user-attachments/assets/dbe000c8-c15c-4ab3-a3a3-63848353fb1f" />

### г) Страница текущих предупреждений RefDepGuard
<img width="1004" height="263" alt="image" src="https://github.com/user-attachments/assets/490c4976-b34a-4c96-8588-1b887846399a" />

## 8. Графический экспорт состояния проекта
При нажатии в меню на кнопку "Экспорт в HTML" или при нажатии комбинации клавиш Alt + H производится формирование графического отчёта в формате .html файла с поддержкой [mermaid](https://docs.mermaidchart.com/mermaid-oss/intro/getting-started.html#native-mermaid-support). 

По результатам экспорта пользователю будет выведен MessageBox с предложением открыть папку с сохранённым экспортом.</br>

<img width="435" height="154" alt="image" src="https://github.com/user-attachments/assets/30b08718-beef-4186-95a5-b0ffb104e4fd" />

Файл будет сохранён по следующему пути и будет иметь следующее название:
````JSON
{название_solution}/reports/graph_type/{DD.MM.YYYY-HH.MM.SS}/{Название solution}_references_report.html
````

Внешний вид графического отчёта:</br>
<img width="1003" height="672" alt="image" src="https://github.com/user-attachments/assets/3c29aaf2-ede6-4c32-88ae-8835e54e9cd0" />
</br>
<img width="942" height="573" alt="image" src="https://github.com/user-attachments/assets/956a0bdb-cd19-47e2-8151-8c3cf2cee5ce" />

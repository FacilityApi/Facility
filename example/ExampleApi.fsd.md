# Example API Definition

```fsd
/// Example service for widgets.
[http(url: "http://local.example.com/v1")]
[csharp(namespace: Facility.ExampleApi)]
service ExampleApi
```

Additional service remarks.

## Heading

Use a primary heading to indicate the member name.

```fsd
/// Gets widgets.
[http(method: GET, path: "/widgets")]
[tag(name: widgets)]
method getWidgets
{
	/// The query.
	[http(name: "q")]
	query: string;

	/// The widget IDs.
	ids: string[]!;

	/// The limit of returned results.
	limit: int32;

	/// The sort field.
	sort: WidgetField;

	/// True to sort descending.
	desc: boolean;

	/// The maximum weight.
	[obsolete]
	maxWeight: double;

	/// The minimum price.
	minPrice: decimal;
}:
{
	/// The widgets.
	widgets: Widget[];

	/// The total number of widgets.
	total: int64;

	/// The total weight.
	[obsolete]
	totalWeight: double;

	/// The pending job.
	[http(from: body, code: 202)]
	job: WidgetJob;
}
```

Additional method remarks.

```fsd
/// Creates a new widget.
[http(method: POST, path: "/widgets")]
[tag(name: widgets)]
method createWidget
{
	/// The widget to create.
	[http(from: body)]
	widget: Widget;
}:
{
	/// The created widget.
	[http(from: body, code: 201)]
	widget: Widget;
}
```

```fsd
/// Gets the specified widget.
[http(method: GET, path: "/widgets/{id}")]
[tag(name: widgets)]
method getWidget
{
	/// The widget ID.
	id: string;

	[http(from: header, name: "If-None-Match")]
	ifNoneMatch: string;
}:
{
	/// The requested widget.
	[http(from: body)]
	widget: Widget;

	[http(from: header)]
	eTag: string;

	[http(from: body, code: 304)]
	notModified: boolean;
}
```

```fsd
/// Deletes the specified widget.
[http(method: DELETE, path: "/widgets/{id}", code: 204)]
[tag(name: widgets), tag(name: admin)]
method deleteWidget
{
	/// The widget ID.
	id: string;
}:
{
}
```

```fsd
/// Edits widget.
[http(method: POST, path: "/widgets/{id}/edit")]
[tag(name: widgets)]
method editWidget
{
	/// The widget ID.
	id: string;

	/// The fields to return.
	[http(from: query)]
	fields: string;

	/// The operations.
	[required]
	ops: object[];

	/// The new weight.
	[obsolete]
	weight: double;
}:
{
	/// The edited widget.
	[http(from: body, code: 200)]
	widget: Widget;

	/// The pending job.
	[http(from: body, code: 202)]
	job: WidgetJob;
}
```

```fsd
/// Gets the specified widgets.
[http(method: POST, path: "/widgets/get")]
[tag(name: widgets)]
method getWidgetBatch
{
	/// The IDs of the widgets to return.
	[http(from: body)]
	ids: string[];
}:
{
	/// The widget results.
	[http(from: body)]
	results: result<Widget>[];
}

/// Gets the widget weight.
[obsolete]
[http(method: GET, path: "/widgets/{id}/weight")]
[tag(name: widgets)]
method getWidgetWeight
{
	/// The widget ID.
	id: string;
}:
{
	/// The widget weight.
	value: double;
}
```

```fsd
/// Gets the widget image.
[http(method: GET, path: "/widgets/{id}/image")]
[tag(name: widgets)]
method getWidgetImage
{
	/// The widget ID.
	id: string;
}:
{
	/// The content of the widget image.
	[http(from: body)]
	content: bytes;

	/// The media type of the widget image.
	[http(from: header, name: Content-Type)]
	type: string;
}
```

```fsd
/// Sets the widget image.
[http(method: PUT, path: "/widgets/{id}/image")]
[tag(name: widgets)]
method setWidgetImage
{
	/// The widget ID.
	id: string;

	/// The content of the widget image.
	[http(from: body)]
	content: bytes;

	/// The media type of the widget image.
	[http(from: header, name: Content-Type)]
	type: string;
}:
{
}
```

```fsd
/// Gets a widget preference.
[http(method: GET, path: "/prefs/{key}")]
method getPreference
{
	/// The preference key.
	key: string;
}:
{
	/// The preference value.
	[http(from: body)]
	value: Preference;
}
```

```fsd
/// Sets a widget preference.
[http(method: PUT, path: "/prefs/{key}")]
method setPreference
{
	/// The preference key.
	key: string;

	/// The preference value.
	[http(from: body)]
	value: Preference;
}:
{
	/// The preference value.
	[http(from: body)]
	value: Preference;
}
```

```fsd
/// Gets service info.
[http(method: GET, path: "/")]
method getInfo
{
}:
{
	/// The name of the service.
	name: string;
}
```

```fsd
/// Demonstrates the default HTTP behavior.
method notRestful
{
}:
{
}
```

```fsd
method kitchen
{
	sink: KitchenSink;
}:
{
	[http(from: header)]
	prices: decimal[];

	[http(from: body)]
	text: string;
}
```

```fsd
method transform
{
	[http(from: body)]
	before: object;
}:
{
	[http(from: body)]
	after: object;
}
```

```fsd
/// Custom errors.
errors ExampleApiErrors
{
	/// The user is not an administrator.
	[http(code: 403)]
	NotAdmin,
}
```

```fsd
/// A widget.
[tag(name: widgets)]
data Widget
{
	/// A unique identifier for the widget.
	id: string;

	/// The name of the widget.
	name: string;

	/// The weight of the widget.
	[obsolete]
	weight: double;

	/// The price of the widget.
	price: decimal;
}
```

Additional DTO remarks.

## Heading

Only top-level headings need to match a member name.

```fsd
/// A widget job.
[tag(name: widgets)]
data WidgetJob
{
	/// A unique identifier for the widget job.
	id: string;
}
```

```fsd
/// A preference.
data Preference
{
	[csharp(name: "IsBoolean")]
	boolean: boolean;

	booleans: boolean[];

	double: double;

	doubles: double[];

	integer: int32;

	integers: int32[];

	string: string;

	strings: string[];

	bytes: bytes;

	byteses: bytes[];

	[tag(name: widgets)]
	widgetField: WidgetField;

	[tag(name: widgets)]
	widgetFields: WidgetField[];

	[tag(name: widgets)]
	widget: Widget;

	[tag(name: widgets)]
	widgets: Widget[];

	[tag(name: widgets)]
	result: result<Widget>;

	[tag(name: widgets)]
	results: result<Widget>[];

	bigInteger: int64;

	bigIntegers: int64[];

	decimal: decimal;

	decimals: decimal[];

	error: error;

	errors: error[];

	object: object;

	objects: object[];

	namedStrings: map<string>;

	[tag(name: widgets)]
	namedWidgets: map<Widget>;
}
```

```fsd
/// Identifies a widget field.
[tag(name: widgets)]
enum WidgetField
{
	/// The 'id' field.
	id,

	/// The 'name' field.
	name,

	/// The 'weight' field.
	[obsolete]
	weight,
}
```

Additional enum remarks.

```fsd
/// An obsolete DTO.
[obsolete]
data ObsoleteData
{
	unused: boolean;
}
```

```fsd
/// An obsolete enum.
[obsolete]
enum ObsoleteEnum
{
	unused,
}
```

```fsd
data KitchenSink
{
	matrix: int32[][][];

	crazy: result<map<string[]>[]>[];

	[obsolete(message: "This field was never used.")]
	oldField: string;
}
```

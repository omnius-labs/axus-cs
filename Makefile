PreviewXamlFile := Views/Windows/Main/MainWindow.axaml

gen-code:
	docker-compose -f ./docker/dev/docker-compose.yml run --rm devenv sh ./eng/gen-code.sh

test:
	docker-compose -f ./docker/dev/docker-compose.yml run --rm devenv sh ./eng/test.sh

update:
	bash ./eng/update.sh

build:
	dotnet build

run-designer: build
	dotnet msbuild ./src/Omnius.Xeus.Ui.Desktop/ /t:Preview /p:XamlFile=$(PreviewXamlFile)

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./pub

.PHONY: build

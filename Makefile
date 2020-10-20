init-tools:
	docker-compose run --rm devenv sh ./eng/init-tools.sh

gen-code:
	docker-compose run --rm devenv sh ./eng/gen-code.sh

test:
	docker-compose run --rm devenv sh ./eng/test.sh

update: format
	docker-compose run --rm devenv sh ./eng/update-tools.sh

format:
	docker-compose run --rm devenv dotnet tool restore
	docker-compose run --rm devenv dotnet tool run dotnet-format

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./pub

.PHONY: init-tools gen-code test update format clean

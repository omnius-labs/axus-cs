init-tools:
	docker-compose run --rm devenv sh ./eng/init-tools.sh

gen-code:
	docker-compose run --rm devenv sh ./eng/gen-code.sh

test:
	docker-compose run --rm devenv sh ./eng/run-test.sh

update-nuget:
	docker-compose run --rm devenv sh ./eng/update-nuget.sh

update-submodule:
	sh ./eng/update-submodule.sh

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./publish

.PHONY: all test clean
